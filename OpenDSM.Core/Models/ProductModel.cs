using OpenDSM.SQL;

namespace OpenDSM.Core.Models;

public class ProductModel
{
    #region Protected Constructors

    protected ProductModel(int id, int userID, string name, string about, int totalDownloads, string videoURL, UserModel user, uint price)
    {
        Id = id;
        UserID = userID;
        Name = name;
        About = about;
        TotalDownloads = totalDownloads;
        VideoURL = videoURL;
        User = user;
        Price = price;
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