// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using System.Diagnostics;

namespace OpenDSM.Core.Handlers;

public class FFmpegHandler
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
        Xabe.FFmpeg.FFmpeg.SetExecutablesPath(FFMpegDirectory);
    }

    #endregion Protected Constructors

    #region Public Methods

    public Task Resize(int width, int height, string file) => Task.Run(() =>
        {
            FileInfo info = new(file);
            DirectoryInfo? dir = Directory.GetParent(file);
            if (dir == null) return;
            string tmp_file = Path.Combine(dir.FullName, $"{info.Name.Replace(info.Extension, "")}_tmp{info.Extension}");
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

    public (int, int) GetSize(string file)
    {
        Xabe.FFmpeg.IMediaInfo info = Xabe.FFmpeg.FFmpeg.GetMediaInfo(file).Result;
        var stream = info.VideoStreams.First();
        return (stream.Width, stream.Height);
    }

    #endregion Public Methods

}