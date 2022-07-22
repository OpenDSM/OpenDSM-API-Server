// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using OpenDSM.SQL;

namespace OpenDSM.Core.Models;

public class ProductModel
{
    #region Protected Constructors

    protected ProductModel(int id, int userID, string gitRepoName, string name, bool useGitReademe, int totalDownloads, int totalWeeklyDownloads, string videoKey, uint price, int[] tags, string[] keywords, PaymentType type)
    {
        Id = id;
        Name = name;
        UseGitReadME = useGitReademe;
        TotalDownloads = totalDownloads;
        TotalWeeklyDownloads = totalWeeklyDownloads;
        User = UserModel.GetByID(userID);
        Price = price;
        YoutubeKey = videoKey;
        Tags = tags;
        Keywords = keywords;
        GitRepositoryName = gitRepoName;
    }

    #endregion Protected Constructors

    #region Public Properties

    public string About
    {
        get
        {
            if (UseGitReadME && GitHandler.HasReadME(GitRepositoryName, new(User.GitUsername, User.GitToken), out string readme))
            {
                return readme;
            }
            using FileStream fs = new(Path.Combine(GetProductDirectory(Id), "about.md"), FileMode.OpenOrCreate, FileAccess.Read);
            using StreamReader reader = new(fs);
            return reader.ReadToEnd();
        }
        set
        {
            using FileStream fs = new(Path.Combine(GetProductDirectory(Id), "about.md"), FileMode.OpenOrCreate, FileAccess.Write);
            using StreamWriter writer = new(fs);
            writer.Write(value);
        }
    }

    public string BannerImage
    {
        get
        {
            return Path.Combine(GetProductDirectory(Id), "banner.jpg");
        }
        set
        {
            byte[] buffer;
            try
            {
                buffer = Convert.FromBase64String(value);
            }
            catch
            {
                log_user.Error("Unable to create banner image from base 64 provided");
                return;
            }
            string file = Path.Combine(GetProductDirectory(Id), "banner.jpg");
            using MemoryStream ms = new(buffer);
            using FileStream fs = new(file, FileMode.OpenOrCreate, FileAccess.Write);
            ms.CopyToAsync(fs).Wait();
            FFmpeg.Instance.Resize(1280, file);
        }
    }

    public string[] GalleryImages
    {
        get
        {
            return Directory.GetFiles(Path.Combine(GetProductDirectory(Id), "gallery"), "*.jpg", SearchOption.TopDirectoryOnly);
        }
        set
        {
            foreach (string image in value)
            {
                string[] sections = image.Split("Name!=", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                byte[] buffer;
                try
                {
                    buffer = Convert.FromBase64String(sections[1]);
                }
                catch
                {
                    log_user.Error("Unable to create banner image from base 64 provided");
                    return;
                }
                string file = Path.Combine(GetProductDirectory(Id), "gallery", $"{sections[0]}");
                using MemoryStream ms = new(buffer);
                using FileStream fs = new(file, FileMode.OpenOrCreate, FileAccess.Write);
                ms.CopyToAsync(fs).Wait();
                FFmpeg.Instance.Resize(1280, file);
            }
        }
    }

    public string GitRepositoryName { get; private set; }
    public bool HasYoutubeVideo => !string.IsNullOrEmpty(YoutubeKey) && !string.IsNullOrWhiteSpace(GetYoutubeDirectURL(YoutubeKey));

    public string IconUrl
    {
        get
        {
            return Path.Combine(GetProductDirectory(Id), "icon.jpg");
        }
        set
        {
            byte[] buffer;
            try
            {
                buffer = Convert.FromBase64String(value);
            }
            catch
            {
                log_user.Error("Unable to create banner image from base 64 provided");
                return;
            }
            string file = Path.Combine(GetProductDirectory(Id), "icon.jpg");
            using MemoryStream ms = new(buffer);
            using FileStream fs = new(file, FileMode.OpenOrCreate, FileAccess.Write);
            ms.CopyToAsync(fs).Wait();
            FFmpeg.Instance.Resize(128, file);
        }
    }

    public int Id { get; private set; }
    public string[] Keywords { get; set; }
    public string Name { get; private set; }
    public Platform[] Platforms { get; set; }
    public uint Price { get; private set; }
    public int[] Tags { get; set; }
    public int TotalDownloads { get; private set; }
    public int TotalWeeklyDownloads { get; private set; }
    public bool UseGitReadME { get; }
    public UserModel User { get; private set; }
    public int UserID { get; private set; }
    public string YoutubeKey { get; private set; }

    #endregion Public Properties

    #region Public Methods

    public static ProductModel? GetByID(int id)
    {
        if (Products.CheckProductExists(id))
        {
            if (Products.GetProductFromID(id, out string name, out string gitRepoName, out bool useGitReadme, out PaymentType type, out int[] tags, out string[] keywords, out int price, out int total_downloads, out int weekly_downloads, out string yt_key, out int owner_id))
            {
                return new(id, owner_id, name, gitRepoName, useGitReadme, total_downloads, weekly_downloads, yt_key, (uint)price, tags, keywords, type);
            }
        }
        return null;
    }

    public static bool TryCreateProduct(string gitRepoName, UserModel user, string name, string yt_key, PaymentType type, int price, string[] keywords, int[] tags, out ProductModel model)
    {
        model = null;
        return Products.Create(user.Id, gitRepoName, name, yt_key, type, price, keywords, tags, out int product_id) && TryGetByID(product_id, out model);
    }

    public static bool TryGetByID(int id, out ProductModel? model)
    {
        model = GetByID(id);
        return model != null;
    }

    #endregion Public Methods
}