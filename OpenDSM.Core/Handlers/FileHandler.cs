using System.Text;

namespace OpenDSM.Core.Handlers;

/// <summary>
/// Handles functions having to do with files
/// </summary>
public static class FileHandler
{

    #region Public Methods

    /// <summary>
    /// Converts Base64 string to image file with specified width and height is set by maintaining aspect ratio
    /// </summary>
    /// <param name="base64">The base64 string</param>
    /// <param name="directory">The output directory of the image file</param>
    /// <param name="file">The file name without extension</param>
    /// <param name="width">The desired width of the file</param>
    public static void CreateImageFromBase64(string base64, string directory, string file, int width)
    {
        if (TryCreateImageFromBase64(base64, directory, file, out string path))
        {
            FFmpegHandler.Instance.ResizeImage(width, path).Wait();
        }
    }

    /// <summary>
    /// Creates an image file using a base64 string
    /// </summary>
    /// <param name="base64">The base64 string</param>
    /// <param name="directory">The output directory of the image file</param>
    /// <param name="file">The file name without extension</param>
    /// <returns>The absolute image file path</returns>
    public static string CreateImageFromBase64(string base64, string directory, string file)
    {
        try
        {
            base64 = base64.Replace("data:image/png;base64,", "");
            base64 = base64.Replace("data:image/jpg;base64,", "");
            string path = Path.Combine(Directory.CreateDirectory(directory).FullName, $"{file}.jpg");
            byte[] buffer = Convert.FromBase64String(base64);
            using MemoryStream ms = new(buffer);
            using FileStream fs = new(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            ms.CopyTo(fs);
            ms.Close();
            fs.Close();
            return path;
        }
        catch (Exception ex)
        {
            log.Error(ex.Message, ex);
            return string.Empty;
        }
    }

    /// <summary>
    /// Checks if the base64 string is valid.
    /// </summary>
    /// <param name="base64">The base64 string</param>
    /// <returns></returns>
    public static bool IsValidBase64(string base64)
    {
        return Convert.TryFromBase64String(base64, Encoding.UTF8.GetBytes(base64), out _);
    }

    /// <summary>
    /// Attempts to create image from base64 string
    /// </summary>
    /// <param name="base64">The base64 string</param>
    /// <param name="directory">The output directory of the image file</param>
    /// <param name="file">The file name without extension</param>
    /// <param name="path">The absolute image file path</param>
    /// <returns>If the conversion was successfull or not</returns>
    public static bool TryCreateImageFromBase64(string base64, string directory, string file, out string path) => !string.IsNullOrEmpty(path = CreateImageFromBase64(base64, directory, file));


    /// <summary>
    /// Attempts to create image from base64 string and resizes it to the specified width.  Maintaining the aspect ratio
    /// </summary>
    /// <param name="base64">The base64 string</param>
    /// <param name="directory">The output directory of the image file</param>
    /// <param name="file">The file name without extension</param>
    /// <param name="width">The width of the file</param>
    /// <param name="path">The absolute image file path</param>
    /// <returns>If the conversion was successfull or not</returns>
    public static bool TryCreateImageFromBase64(string base64, string directory, string file, int width, out string path)
    {
        if (TryCreateImageFromBase64(base64, directory, file, out path))
        {
            FFmpegHandler.Instance.ResizeImage(width, path).Wait();
            return true;
        }
        return false;
    }

    #endregion Public Methods
}
