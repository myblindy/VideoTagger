using FFmpeg.AutoGen;
using FFmpeg.Loader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VideoTagger.Models;

using static FFmpeg.AutoGen.ffmpeg;

namespace VideoTagger.Services;

public sealed partial class VideoProcessingService(
    MainModel mainModel, DbService dbService, ILogger<VideoProcessingService> logger)
    : IHostedService
{
    public async Task UpdateVideosAsync()
    {
        var allMemberSearchValues = mainModel.Groups.ToDictionary(g => g, g =>
            SearchValues.Create(g.Members.Select(m => m.Name).ToArray(), StringComparison.OrdinalIgnoreCase));

        await Task.WhenAll(mainModel.Folders.SelectMany(f => Directory.EnumerateFiles(f.Path, "*", SearchOption.AllDirectories))
            .Where(f => Path.GetExtension(f.ToLowerInvariant()) is ".avi" or ".mkv" or ".mp4" or ".mov" or ".webm")
            .Select(f => Task.Run(() => ProcessVideo(f))));
        await dbService.CommitVideoCacheEntryUpdatesAsync();

        //foreach (var f in mainModel.Folders.SelectMany(f => Directory.EnumerateFiles(f.Path, "*", SearchOption.AllDirectories))
        //    .Where(f => Path.GetExtension(f.ToLowerInvariant()) is ".avi" or ".mkv" or ".mp4" or ".mov" or ".webm"))
        //{
        //    ProcessVideo(f);
        //}

        void ProcessVideo(string filePath)
        {
            var result = new MainModelVideoCacheEntry { Path = filePath };
            Func<byte[]?>? coverImageBytes = null;

            try
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                if (VideoPartsRegex().Match(fileName) is not { Success: true } m) return;
                var videoParts = m.Groups!;

                // date
                if (videoParts[1].Success == true
                    && DateTime.TryParseExact(videoParts[1].ValueSpan, ["yyyyMMdd", "yyMMdd"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var foundDate))
                {
                    result.Date = foundDate;
                }

                // group name
                var group = mainModel.Groups.FirstOrDefault(g =>
                    g.Name.Equals(videoParts[2].ValueSpan, StringComparison.OrdinalIgnoreCase)
                    || g.AlternativeNames.Any(an => an.Equals(videoParts[2].ValueSpan, StringComparison.OrdinalIgnoreCase)));

                if (group is null)
                {
                    // do stuff for unknown group
                    return;
                }

                // split strings by members
                var memberSearchValues = allMemberSearchValues[group];
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
                                memberStrings.Add((currentMember, videoParts[3].ValueSpan[currentStartIndex..index].ToString()));

                            currentMember = member;
                            currentStartIndex = index += member.Name.Length;
                            break;
                        }
                }
                if (currentMember is not null)
                    memberStrings.Add((currentMember, videoParts[3].ValueSpan[currentStartIndex..].ToString()));

                // members
                Dictionary<MainModelGroupMember, Dictionary<(MainModelCategory category, MainModelCategoryItem item), object>> tags = [];
                foreach (var (member, @string) in memberStrings)
                {
                    tags.TryAdd(member, []);
                    foreach (var category in mainModel.Categories)
                        foreach (var categoryItem in category.Items)
                            if (categoryItem.IsBoolean)
                            {
                                if (Regex.IsMatch(@string, categoryItem.BooleanRegex ?? @$"\b{Regex.Escape(categoryItem.Name)}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                                {
                                    tags[member][(category, categoryItem)] = true;
                                    break;
                                }
                            }
                            else
                            {
                                foreach (var enumValue in categoryItem.EnumValues)
                                {
                                    if (enumValue.Regex is not null
                                        && Regex.IsMatch(@string, enumValue.Regex, RegexOptions.IgnoreCase))
                                    {
                                        tags[member][(category, categoryItem)] = enumValue;
                                        break;
                                    }
                                }
                            }
                }

                // put it in the result format
                foreach (var (member, memberTags) in tags)
                    result.Tags.Add(new MainModelVideoCacheTag
                    {
                        Member = member,
                        Items = new(memberTags.Select(t => new MainModelVideoCacheTagItem
                        {
                            CategoryItem = t.Key.item,
                            BooleanValue = t.Value is bool b && b,
                            EnumValue = t.Value as MainModelCategoryItemEnumValue
                        }))
                    });

                // cover image
                coverImageBytes = () => GetVideoCoverImageBytes(filePath, AVHWDeviceType.AV_HWDEVICE_TYPE_NONE);
            }
            finally
            {
                dbService.QueueVideoCacheEntryUpdate(result, coverImageBytes);
            }
        }
    }

    [GeneratedRegex(@"^\s*(?:(\d+)\s+)?(.+?)\s+-\s*(.*)$")]
    private static partial Regex VideoPartsRegex();

    static unsafe byte[] GetVideoCoverImageBytes(string videoPath, AVHWDeviceType hwDeviceType = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
    {
        // open video
        var formatContext = avformat_alloc_context();
        avformat_open_input(&formatContext, videoPath, null, null).ThrowIfError();
        avformat_find_stream_info(formatContext, null).ThrowIfError();

        AVCodec* codec = null;
        var streamIndex = av_find_best_stream(formatContext, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, &codec, 0)
            .ThrowIfError();
        var codecContext = avcodec_alloc_context3(codec);

        if (hwDeviceType != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
            av_hwdevice_ctx_create(&codecContext->hw_device_ctx, hwDeviceType, null, null, 0).ThrowIfError();

        avcodec_parameters_to_context(codecContext, formatContext->streams[streamIndex]->codecpar).ThrowIfError();
        avcodec_open2(codecContext, codec, null).ThrowIfError();

        var frameSize = new Size(codecContext->width, codecContext->height);
        var inputPixelFormat = codecContext->pix_fmt;

        var packet = av_packet_alloc();
        var frame = av_frame_alloc();
        AVFrame* receivedFrame = default;

        // decode the next frame
        var error = 0;
        do
        {
            try
            {
                do
                {
                    av_packet_unref(packet);
                    error = av_read_frame(formatContext, packet);
                    if (error == AVERROR_EOF)
                        goto gotFrame;
                    error.ThrowIfError();
                } while (packet->stream_index != streamIndex);

                avcodec_send_packet(codecContext, packet).ThrowIfError();
            }
            finally { av_packet_unref(packet); }

            error = avcodec_receive_frame(codecContext, frame);
        } while (error == AVERROR(EAGAIN));
        error.ThrowIfError();

gotFrame:
        if (codecContext->hw_device_ctx is not null)
        {
            receivedFrame = av_frame_alloc();
            av_hwframe_transfer_data(receivedFrame, frame, 0).ThrowIfError();
            av_frame_free(&frame);
        }
        else
            receivedFrame = frame;

        // compute the scaled down size
        const int maxWidth = 256, maxHeight = 256;
        Size scaledFrameSize;

        var aspectRatio = (double)frameSize.Width / frameSize.Height;
        if (aspectRatio > 1)
            scaledFrameSize = new Size(maxWidth, (int)(maxWidth / aspectRatio));
        else
            scaledFrameSize = new Size((int)(maxHeight * aspectRatio), maxHeight);

        // process frame
        const AVPixelFormat destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_BGRA;
        var convertContext = sws_getContext(
            frameSize.Width, frameSize.Height, inputPixelFormat,
            scaledFrameSize.Width, scaledFrameSize.Height, destinationPixelFormat,
            SWS_BICUBIC, null, null, null);

        var convertedFrameBufferSize = av_image_get_buffer_size(destinationPixelFormat,
            scaledFrameSize.Width, scaledFrameSize.Height, 1);
        byte* convertedFrameBuffer = (byte*)av_malloc((ulong)convertedFrameBufferSize);

        byte_ptrArray4 destinationData = default;
        int_array4 destinationLineSize = default;
        av_image_fill_arrays(ref destinationData, ref destinationLineSize, convertedFrameBuffer, destinationPixelFormat,
            scaledFrameSize.Width, scaledFrameSize.Height, 1).ThrowIfError();

        sws_scale(convertContext, receivedFrame->data, receivedFrame->linesize, 0, frameSize.Height,
            destinationData, destinationLineSize).ThrowIfError();

        // convert the frame to a bitmap and write it to the stream
        using var image = Image.LoadPixelData(
            new ReadOnlySpan<Bgra32>(destinationData[0], convertedFrameBufferSize),
            scaledFrameSize.Width, scaledFrameSize.Height);
        using var ms = new MemoryStream();
        image.SaveAsJpeg(ms);

        // cleanup
        av_freep(&convertedFrameBuffer);
        sws_freeContext(convertContext);
        av_frame_free(&receivedFrame);
        av_packet_free(&packet);
        avcodec_free_context(&codecContext);
        avformat_close_input(&formatContext);

        return ms.ToArray();
    }

    av_log_set_callback_callback? ffmpegLogCallback;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier",
        Justification = "Not readonly, modified in native code")]
    int ffmpegLogPrintPrefix = 1;
    public Task StartAsync(CancellationToken cancellationToken)
    {
        FFmpegLoader.SearchApplication()
            .ThenSearchPaths("./runtimes/win-x64/native")
            .ThenSearchSystem()
            .Load();

        unsafe
        {
            ffmpegLogCallback = (avcl, level, fmt, va) =>
            {
                var logLevel = level switch
                {
                    AV_LOG_PANIC or AV_LOG_FATAL or AV_LOG_ERROR => LogLevel.Error,
                    AV_LOG_WARNING => LogLevel.Warning,
                    AV_LOG_INFO => LogLevel.Information,
                    AV_LOG_VERBOSE or AV_LOG_DEBUG => LogLevel.Debug,
                    _ => LogLevel.Trace
                };
                if (!logger.IsEnabled(logLevel))
                    return;

                const int lineSize = 1024;
                byte* line = stackalloc byte[lineSize];
                fixed (int* ffmpegLogPrintPrefix = &this.ffmpegLogPrintPrefix)
                    av_log_format_line(avcl, level, fmt, va, line, lineSize, ffmpegLogPrintPrefix);
                logger.Log(logLevel, "[FFmpeg] {Message}", Marshal.PtrToStringUTF8((IntPtr)line) ?? string.Empty);
            };
            av_log_set_callback(ffmpegLogCallback);
            av_log_set_level(
#if DEBUG   
                AV_LOG_DEBUG
#else
                ffmpeg.AV_LOG_WARNING
#endif
                );

            av_log(null, AV_LOG_INFO, $"FFmpeg initialized, version: {av_version_info()}");
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}

static class VideoProcessingServiceExtensions
{
    public static IServiceCollection AddVideoProcessingService(this IServiceCollection services) => services
        .AddSingleton<VideoProcessingService>()
        .AddHostedService(p => p.GetRequiredService<VideoProcessingService>());
}

file static class FFmpegExtensions
{
    public static unsafe int ThrowIfError(this int error,
        [CallerMemberName] string? callerMemberName = null,
        [CallerLineNumber] int callerLineNumber = 0)
    {
        if (error < 0)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(1024);
            try
            {
                fixed (byte* bufferPtr = &buffer[0])
                {
                    av_strerror(error, bufferPtr, (ulong)buffer.Length);
                    throw new ApplicationException($"FFmpeg error in {callerMemberName}:{callerLineNumber}: {Marshal.PtrToStringUTF8(new(bufferPtr))}");
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        return error;
    }
}