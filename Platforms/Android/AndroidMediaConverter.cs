#if ANDROID
namespace Nana_Format_Factory
{
    public class AndroidMediaConverter : IMediaConverter
    {
        public async Task<string?> ConvertAsync(string inputPath)
        {
            var ext = Path.GetExtension(inputPath).ToLowerInvariant();
            var isAudio = ext is ".wav" or ".aac" or ".flac" or ".ogg" or ".wma";

            var outputPath = Path.ChangeExtension(
                inputPath,
                isAudio ? ".mp3" : ".mp4"
            );

            var command = isAudio
                ? $"-y -i \"{inputPath}\" -vn -acodec libmp3lame \"{outputPath}\""
                : $"-y -i \"{inputPath}\" -c:v libx264 -preset ultrafast -crf 28 -c:a aac \"{outputPath}\"";

            var session = FFMpegKit.Droid.FFmpegKit.Execute(command);

            if (session!.ReturnCode!.IsValueSuccess)
            {
                return outputPath;
            }

            return null;
        }
    }
}
#endif
