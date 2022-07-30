// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using OpenDSM.Core.Handlers;
using OpenDSM.SQL;

namespace OpenDSM.Core.Models;

public class ProductModel
{
    #region Protected Constructors

    protected ProductModel(int id, int userID, string gitRepoName, string name, bool useGitReademe, string videoKey, uint price, int[] tags, string[] keywords, bool subscription)
    {
        Id = id;
        Name = name;
        UseGitReadME = useGitReademe;
        TotalDownloads = 0;
        TotalWeeklyDownloads = 0;
        User = UserModel.GetByID(userID);
        Price = price;
        YoutubeKey = videoKey;
        Tags = tags;
        Keywords = keywords;
        GitRepositoryName = gitRepoName;
        Subscription = subscription;
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
            FileHandler.CreateImageFromBase64(value, GetProductDirectory(Id), "banner", 1280);
        }
    }

    public string[] GalleryImages
    {
        get
        {
            return Directory.GetFiles(Directory.CreateDirectory(Path.Combine(GetProductDirectory(Id), "gallery")).FullName, "*.jpg", SearchOption.TopDirectoryOnly);
        }
        set
        {
            for (int i = 0; i < value.Length; i++)
            {
                FileHandler.CreateImageFromBase64(value[i], Path.Combine(GetProductDirectory(Id), "gallery"), $"gallery_{i}", 1280);
            }
        }
    }
    public bool Subscription { get; private set; }
    public string GitRepositoryName { get; private set; }
    public bool HasYoutubeVideo => YTHandler.IsValidYoutubeKey(YoutubeKey);

    public string IconUrl
    {
        get
        {
            return Path.Combine(GetProductDirectory(Id), "icon.jpg");
        }
        set
        {
            FileHandler.CreateImageFromBase64(value, GetProductDirectory(Id), "icon", 128);
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
            if (Products.GetProductFromID(id, out string name, out string gitRepoName, out bool useGitReadme, out bool subscription, out int[] tags, out string[] keywords, out int price, out string yt_key, out int owner_id))
            {
                return new(id, owner_id, gitRepoName, name, useGitReadme,yt_key, (uint)price, tags, keywords, subscription);
            }
        }
        return null;
    }

    public static bool TryCreateProduct(string gitRepoName, UserModel user, string name, string yt_key, bool subscription, bool use_git_readme, int price, string[] keywords, int[] tags, out ProductModel model)
    {
        model = null;
        return Products.Create(user.Id, gitRepoName, name, yt_key, subscription, use_git_readme, price, keywords, tags, out int product_id) && TryGetByID(product_id, out model);
    }

    public static bool TryGetByID(int id, out ProductModel? model)
    {
        model = GetByID(id);
        return model != null;
    }

    #endregion Public Methods
}