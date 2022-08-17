// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace OpenDSM.Core.Handlers;

public static class YTHandler
{

    #region Public Methods

    public static async Task<IReadOnlyList<YoutubeExplode.Playlists.PlaylistVideo>?> GetChannelVideos(string channelId)
    {
        YoutubeClient client = new();
        Channel? channel = null;
        try
        {
            channel = await client.Channels.GetByUserAsync($"https://youtube.com/user/{channelId}");
        }
        catch
        {
            try
            {
                channel = await client.Channels.GetByUserAsync($"https://youtube.com/channel/{channelId}");
            }
            catch
            {
                try
                {
                    channel = await client.Channels.GetByUserAsync($"https://youtube.com/c/{channelId}");
                }
                catch
                {
                    try
                    {
                        channel = await client.Channels.GetByUserAsync($"https://youtube.com/u/{channelId}");
                    }
                    catch
                    {
                        try
                        {
                            channel = await client.Channels.GetBySlugAsync($"https://youtube.com/user/{channelId}");
                        }
                        catch
                        {
                            try
                            {
                                channel = await client.Channels.GetBySlugAsync($"https://youtube.com/channel/{channelId}");
                            }
                            catch
                            {
                                try
                                {
                                    channel = await client.Channels.GetBySlugAsync($"https://youtube.com/c/{channelId}");
                                }
                                catch
                                {
                                    try
                                    {

                                        channel = await client.Channels.GetBySlugAsync($"https://youtube.com/u/{channelId}");
                                    }
                                    catch
                                    {
                                        return null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return await client.Channels.GetUploadsAsync($"https://youtube.com/channel/{channel.Id}");
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

    #endregion Public Methods

}