using OpenDSM.SQL;
using YoutubeExplode;
using System.Linq;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace OpenDSM.Core.Models;

public class ProductModel
{
    #region Protected Constructors

    protected ProductModel(int id, int userID, string name, string about, int totalDownloads, string videoKey, UserModel user, uint price)
    {
        Id = id;
        UserID = userID;
        Name = name;
        About = about;
        TotalDownloads = totalDownloads;
        User = user;
        Price = price;

        try
        {
            string youtubeURL = $"https://youtube.com/watch?v={videoKey}";
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

            VideoURL = url;
        }
        catch
        {
            VideoURL = "";
        }
    }

    #endregion Protected Constructors

    #region Public Properties

    public string About { get; private set; }
    public string BannerUrl { get; set; }
    public string[] GalleryImages { get; set; }
    public string IconUrl { get; set; }
    public int Id { get; private set; }
    public string[] Keywords { get; set; }
    public string Name { get; private set; }
    public Platform[] Platforms { get; set; }
    public uint Price { get; private set; }
    public int[] Tags { get; set; }
    public int TotalDownloads { get; private set; }
    public UserModel User { get; private set; }
    public int UserID { get; private set; }
    public string VideoURL { get; private set; }

    #endregion Public Properties

    #region Public Methods

    public static ProductModel? GetByID(int id)
    {
        return null;
    }

    public static bool TryGetByID(int id, out ProductModel? model)
    {
        model = GetByID(id);
        return model != null;
    }

    #endregion Public Methods
}