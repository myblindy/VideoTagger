using FFmpeg.AutoGen;

namespace VideoTagger.Helpers;

static class FFmpegHelpers
{
    public static void InitializeBinaries()
    {
        DynamicallyLoadedBindings.Initialize();

        ffmpeg.av_log_set_level(ffmpeg.AV_LOG_ERROR);
    }
}
