// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace OpenDSM.Core;

public static class YTHandler
{
    #region Public Methods

    public static bool IsValidYoutubeKey(string key) => TryGetYoutubeDirectURL(key, out _);

    public static bool TryGetYoutubeDirectURL(string id, out Uri url)
    {
        string urlString = GetYoutubeDirectURL(id);
        if (!string.IsNullOrEmpty(urlString))
        {
            url = new(urlString);
            return true;
        }
        url = null;
        return false;
    }

    public static string GetYoutubeDirectURL(string id)
    {
        try
        {
            string youtubeURL = $"https://youtube.com/watch?v={id}";
            YoutubeClient client = new();
            VideoClient videoClient = client.Videos;
            StreamClient videoStreams = videoClient.Streams;
            StreamManifest? manifest = videoStreams.GetManifestAsync(youtubeURL).Result;
            IEnumerable<MuxedStreamInfo> muxedStreams = manifest.GetMuxedStreams();

            string url = "";
            int area = -1;
            foreach (MuxedStreamInfo videoStream in from videoStream in muxedStreams where videoStream.VideoResolution.Area > area select videoStream)
            {
                area = videoStream.VideoResolution.Area;
                url = videoStream.Url;
            }

            return url;
        }
        catch
        {
            return "";
        }
    }
    #endregion Public Methods
}