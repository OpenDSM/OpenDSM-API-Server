using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDSM.Core.Handlers;

public static class FileHandler
{

    #region Public Methods

    public static void CreateImageFromBase64(string base64, string directory, string file, int width)
    {
        if (TryCreateImageFromBase64(base64, directory, file, out string path))
        {
            FFmpegHandler.Instance.Resize(width, -1, path).Wait();
        }
    }

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

    public static bool TryCreateImageFromBase64(string base64, string directory, string file, out string path) => !string.IsNullOrEmpty(path = CreateImageFromBase64(base64, directory, file));
    public static bool TryCreateImageFromBase64(string base64, string directory, string file, int width, out string path)
    {
        if (TryCreateImageFromBase64(base64, directory, file, out path))
        {
            FFmpegHandler.Instance.Resize(width, -1, path).Wait();
            return true;
        }
        return false;
    }

    #endregion Public Methods

}
