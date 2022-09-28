// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace OpenDSM.Core.Handlers;

public static class YTHandler
{

    #region Public Methods

    /// <summary>
    /// Gets a list of all youtube videos based on channel id
    /// </summary>
    /// <param name="channelId">The channel id</param>
    /// <returns></returns>
    public static async Task<IReadOnlyList<PlaylistVideo>?> GetChannelVideos(string channelId, int count = 20, int page = 0)
    {
        YoutubeClient client = new();
        Channel? channel = null;
        try
        {
            channel = await client.Channels.GetByUserAsync(channelId);
        }
        catch
        {
            channel = await client.Channels.GetBySlugAsync(channelId);
        }
        if (channel == null)
        {
            string[] slugs = { "user", "channel", "c", "u" };
            foreach (string slug in slugs)
            {
                try
                {
                    channel = await client.Channels.GetByUserAsync($"https://youtube.com/{slug}/{channelId}");
                    break;
                }
                catch
                {

                    try
                    {
                        channel = await client.Channels.GetBySlugAsync($"https://youtube.com/{slug}/{channelId}");
                        break;
                    }
                    catch
                    {
                        channel = null;
                        continue;
                    }
                }
            }
        }
        if (channel != null)
        {
            IReadOnlyList<PlaylistVideo> uploads = await client.Channels.GetUploadsAsync(channel.Id);

            return uploads.Skip(page * count).Take(count).ToArray();
        }
        return Array.Empty<PlaylistVideo>();
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

    public static bool IsValidYoutubeKey(string key) => TryGetYoutubeDirectURL(key, out _);

    public static bool TryGetYoutubeDirectURL(string id, out Uri? url)
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

    #endregion Public Methods

}