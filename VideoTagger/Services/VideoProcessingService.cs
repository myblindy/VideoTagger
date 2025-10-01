using Microsoft.Extensions.DependencyInjection;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoTagger.Models;

namespace VideoTagger.Services;

public sealed partial class VideoProcessingService(MainModel mainModel)
{
    public Task UpdateVideosAsync() =>
        Task.WhenAll(mainModel.Folders.SelectMany(f => Directory.EnumerateFiles(f.Path, "*", SearchOption.AllDirectories))
            .Where(f => Path.GetExtension(f.ToLowerInvariant()) is ".avi" or ".mkv" or ".mp4" or ".mov" or ".webm")
            .Select(f => Task.Run(() => ProcessVideo(f))));

    void ProcessVideo(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        if (VideoPartsRegex().Match(fileName) is not { Success: true } m) return;
        var videoParts = m.Groups!;

        // date
        DateTime? date;
        if (videoParts[1].Success == true
            && DateTime.TryParseExact(videoParts[1].ValueSpan, ["yyyyMMdd", "yyMMdd"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var foundDate))
        {
            date = foundDate;
        }

        // group name
        var group = mainModel.Groups.FirstOrDefault(g => g.Name.Equals(videoParts[2].ValueSpan, StringComparison.OrdinalIgnoreCase));

        if (group is null)
        {
            // do stuff for unknown group
            return;
        }

        // split strings by members
        var memberSearchValues = SearchValues.Create(group.Members.Select(m => m.Name).ToArray(), StringComparison.OrdinalIgnoreCase);
        List<(MainModelGroupMember member, string @string)> memberStrings = [];
        MainModelGroupMember? currentMember = default;
        int currentStartIndex = 0;
        for (int index = videoParts[3].ValueSpan.IndexOfAny(memberSearchValues);
            index >= 0;
            index = videoParts[3].ValueSpan[index..].IndexOfAny(memberSearchValues) is { } newIndex ? newIndex >= 0 ? index + newIndex : -1 : -1)
        {
            foreach (var member in group.Members)
                if (videoParts[3].ValueSpan.Slice(index, member.Name.Length).Equals(member.Name, StringComparison.OrdinalIgnoreCase))
                {
                    if (currentMember is not null)
                        memberStrings.Add((member, videoParts[3].ValueSpan[currentStartIndex..index].ToString()));

                    currentMember = member;
                    currentStartIndex = index += member.Name.Length;
                    break;
                }
        }
        if (currentMember is not null)
            memberStrings.Add((currentMember, videoParts[3].ValueSpan[currentStartIndex..].ToString()));

        // members
        Dictionary<MainModelGroupMember, Dictionary<MainModelCategoryItem, object>> tags = [];
        foreach (var (member, @string) in memberStrings)
        {
            foreach (var category in mainModel.Categories)
                foreach (var categoryItem in category.Items)
                    if (categoryItem.IsBoolean)
                    {
                        if ((categoryItem.BooleanRegex is not null && Regex.IsMatch(@string, categoryItem.BooleanRegex, RegexOptions.IgnoreCase))
                            || (categoryItem.BooleanRegex is null && @string.Contains(categoryItem.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            tags.TryAdd(member, []);
                            tags[member][categoryItem] = true;
                        }
                    }
                    else
                    {
                        foreach (var enumValue in categoryItem.EnumValues)
                        {
                            if (enumValue.Regex is not null
                                && Regex.IsMatch(@string, enumValue.Regex, RegexOptions.IgnoreCase))
                            {
                                tags.TryAdd(member, []);
                                tags[member][categoryItem] = enumValue.EnumValue;
                            }
                        }
                    }
        }
    }

    [GeneratedRegex(@"^\s*(?:(\d+)\s+)?(.+?)\s+-\s*(.*)$")]
    private static partial Regex VideoPartsRegex();
}

static class VideoProcessingServiceExtensions
{
    public static IServiceCollection AddVideoProcessingService(this IServiceCollection services) =>
        services.AddSingleton<VideoProcessingService>();
}