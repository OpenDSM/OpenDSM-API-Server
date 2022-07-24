// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using System.Diagnostics;

namespace OpenDSM.Core.Handlers;

internal class FFmpegHandler
{
    #region Public Fields

    public static FFmpegHandler Instance = Instance ??= new();

    #endregion Public Fields

    #region Private Fields

    private string ffmpeg_exe;

    #endregion Private Fields

    #region Protected Constructors

    protected FFmpegHandler()
    {
        if (!Directory.GetFiles(FFMpegDirectory, "*ffmpeg*", SearchOption.AllDirectories).Any())
        {
            Xabe.FFmpeg.Downloader.FFmpegDownloader.GetLatestVersion(Xabe.FFmpeg.Downloader.FFmpegVersion.Official, FFMpegDirectory).Wait();
        }
        ffmpeg_exe = Directory.GetFiles(FFMpegDirectory, "*ffmpeg*", SearchOption.AllDirectories).First();
    }

    #endregion Protected Constructors

    #region Public Methods

    public Task Resize(int width, int height, string file) => Task.Run(() =>
        {
            FileInfo info = new(file);
            string tmp_file = Path.Combine(Directory.GetParent(file).FullName, $"{info.Name.Replace(info.Extension, "")}_tmp{info.Extension}");
            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = ffmpeg_exe,
                    Arguments = $"-y -i \"{file}\" -loglevel quiet -vf scale={width}:{height} \"{tmp_file}\""
                }
            };
            process.Start();
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                Thread.Sleep(500);
                File.Move(tmp_file, file, true);
            }
            else
            {
                log.Error($"FFmpeg exited with code {process.ExitCode}");
            }
        });

    #endregion Public Methods
}