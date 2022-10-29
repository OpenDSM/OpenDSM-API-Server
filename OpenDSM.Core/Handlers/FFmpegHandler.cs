// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using System.Diagnostics;
using System.IO;

namespace OpenDSM.Core.Handlers;

/// <summary>
/// Handles all FFmpeg actions
/// </summary>
public class FFmpegHandler
{

    #region Public Fields

    /// <summary>
    /// A singleton instance of FFmpegHandler
    /// </summary>
    /// <returns></returns>
    public static FFmpegHandler Instance = Instance ??= new();

    #endregion Public Fields

    #region Public Methods

    /// <summary>
    /// Returns the width and height of the image file
    /// </summary>
    /// <param name="file">The absolute path to the image file</param>
    /// <returns>width, height</returns>
    public (int width, int height) GetSize(string file)
    {
        Xabe.FFmpeg.IMediaInfo info = Xabe.FFmpeg.FFmpeg.GetMediaInfo(file).Result;
        var stream = info.VideoStreams.First();
        return (stream.Width, stream.Height);
    }

    /// <summary>
    /// Creates an instance of the image with the desired dimensions and then overwrites the original.
    /// </summary>
    /// <param name="width">The desired width</param>
    /// <param name="height">The desired height</param>
    /// <param name="file">The absolute path to the image file</param>
    /// <returns></returns>
    public Task ResizeImage(int width, string file) =>
    Task.Run(() =>
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
                    Arguments = $"-y -i \"{file}\" -loglevel quiet -vf \"scale={width}:-1\" \"{tmp_file}\""
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

    #region Protected Constructors

    /// <summary>
    /// Initializes FFmpeg
    /// </summary>
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

    #region Private Fields

    private string ffmpeg_exe;

    #endregion Private Fields

}