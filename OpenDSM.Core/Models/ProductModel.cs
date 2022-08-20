// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using OpenDSM.Core.Handlers;
using OpenDSM.SQL;
using System.Net;

namespace OpenDSM.Core.Models;

public class ProductModel
{

    #region Protected Constructors

    protected ProductModel(int id, int userID, string gitRepoName, string name, string summery, bool useGitReademe, string videoKey, uint price, int[] tags, string[] keywords, bool subscription, int pageViews, DateTime posted)
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
        ShortSummery = summery;
        TotalPageViews = pageViews;
        Posted = posted;
        PopulateVersionsFromDB();
    }

    #endregion Protected Constructors

    #region Public Properties

    public string About
    {
        get
        {
            string path = Path.Combine(GetProductDirectory(Id), "about.md");

            if (UseGitReadME)
            {
                if (!File.Exists(path))
                {
                    if (GitHandler.HasReadME(GitRepositoryName, User.GitCredentials, out string readme))
                    {
                        About = readme;
                        return readme;
                    }
                }
            }

            using FileStream fs = new(path, FileMode.OpenOrCreate, FileAccess.Read);
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

    public Dictionary<string, float> Coupon { get; private set; }
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
    public bool OnSale => SalePrice != -1;
    public Platform[] Platforms { get; set; }
    public DateTime Posted { get; set; }
    public uint Price { get; private set; }
    public byte Rating { get; private set; }
    public List<ReviewModel> Reviews { get; private set; }
    public float SalePrice { get; private set; } = -1;
    public string ShortSummery { get; set; }
    public bool Subscription { get; private set; }
    public int[] Tags { get; set; }
    public int TotalDownloads { get; private set; }
    public int TotalPageViews { get; private set; }
    public int TotalWeeklyDownloads { get; private set; }
    public bool UseGitReadME { get; }
    public UserModel User { get; private set; }
    public Dictionary<long, VersionModel> Versions { get; private set; }
    public float ViewDownloadRatio => TotalDownloads / TotalPageViews;
    public string YoutubeKey { get; private set; }

    #endregion Public Properties

    #region Public Methods

    public static ProductModel? GetByID(int id)
    {
        try
        {
            if (Products.GetProductFromID(id, out string name, out string gitRepoName, out string summery, out bool useGitReadme, out bool subscription, out int[] tags, out string[] keywords, out int price, out string yt_key, out int owner_id, out int pageViews, out DateTime posted))
            {
                return new(id, owner_id, gitRepoName, name, summery, useGitReadme, yt_key, (uint)price, tags, keywords, subscription, pageViews, posted);
            }
        }
        catch
        {

        }
        return null;
    }

    public static bool TryCreateProduct(string gitRepoName, UserModel user, string name, string yt_key, bool subscription, bool use_git_readme, int price, string[] keywords, int[] tags, out ProductModel model)
    {
        model = null;
        if (Products.Create(user.Id, gitRepoName, name, yt_key, subscription, use_git_readme, price, keywords, tags, out int product_id) && TryGetByID(product_id, out model))
        {
            GitHandler.CreateWebHook(user.GitCredentials, model);
            model.PopulateVersionsFromGit();
            return true;
        }
        return false;
    }

    public static bool TryGetByID(int id, out ProductModel? model)
    {
        model = GetByID(id);
        return model != null;
    }

    public void AddPageView()
    {
        Products.AddPageView(Id);
    }

    public void PopulateReviews()
    {
        Reviews = new();
        int[] review_ids = SQL.Reviews.GetReviewsByProductID(Id);
        foreach (int id in review_ids)
        {
            if (SQL.Reviews.GetReviewByID(id, out _, out byte rating, out string summery, out string body, out DateTime posted, out int user_id))
            {
                Reviews.Add(new()
                {
                    Posted = posted,
                    Product = this,
                    Rating = rating,
                    Summery = summery,
                    User = UserModel.GetByID(user_id)
                });
            }
        }
    }

    public void PopulateVersionsFromDB()
    {
        long[] version_ids = SQL.Versions.GetVersionsByProductID(Id);
        if (version_ids.Any())
        {
            TotalDownloads = TotalWeeklyDownloads = 0;
            List<VersionModel> versions_list = new();
            List<Platform> platforms = new();
            try
            {
                foreach (long version_id in version_ids)
                {
                    if (SQL.Versions.GetVersionByID(version_id, Id, out string name, out byte releaseTypeB, out byte[] platformsB, out string changelog, out DateTime posted))
                    {
                        ReleaseType type = (ReleaseType)releaseTypeB;
                        List<PlatformVersion> platformVersions = new();
                        foreach (byte p in platformsB)
                        {
                            Platform platform = (Platform)p;

                            if (!platforms.Contains(platform))
                            {
                                platforms.Add(platform);
                            }
                            if (SQL.Versions.GetPlatformVersionByID(p, version_id, out string download_url, out int total_downloads, out int weekly_downloads, out long filesize))
                            {
                                platformVersions.Add(new(platform, download_url, total_downloads, weekly_downloads, filesize));
                            }
                        }

                        VersionModel version = new(version_id, Id, name, type, platformVersions, changelog, posted);
                        versions_list.Add(version);
                        TotalDownloads += version.TotalDownloads;
                        TotalWeeklyDownloads += version.WeeklyDownloads;
                    }
                }
            }
            catch { }
            Platforms = platforms.ToArray();
            Versions = versions_list.OrderByDescending(i => i.Posted).ToDictionary(i => i.ID);
        }
        else
        {
            PopulateVersionsFromGit();
        }
    }

    public void PopulateVersionsFromGit()
    {
        TotalDownloads = TotalWeeklyDownloads = 0;
        List<VersionModel> versions_list = new();
        List<Platform> platforms = new();
        try
        {
            long[] remoteVersions = GitHandler.GitReleases(GitRepositoryName, User.GitCredentials).Result;
            foreach (long version_id in SQL.Versions.GetVersionsByProductID(Id))
            {
                if (!remoteVersions.Contains(version_id))
                {
                    SQL.Versions.RemoveVersion(Id, version_id);
                }
            }
            foreach (long version_id in remoteVersions)
            {
                if (GitHandler.GetVersionFromID(GitRepositoryName, (int)version_id, Id, User.GitCredentials, out VersionModel version))
                {
                    versions_list.Add(version);
                    foreach (PlatformVersion platform in version.Platforms)
                    {
                        if (!platforms.Contains(platform.platform))
                        {
                            platforms.Add(platform.platform);
                        }
                        if (!SQL.Versions.PlatformExists(version_id, (byte)platform.platform))
                        {
                            SQL.Versions.CreatePlatformVersion(platform.platform, platform.downloadUrl, version_id, platform.file_size);
                        }
                    }
                    if (!SQL.Versions.VersionExists(version_id, Id))
                    {
                        SQL.Versions.CreateVersion(version_id, Id, version.Name, (byte)version.Type, version.Changelog, version.Posted);
                    }
                    TotalDownloads += version.TotalDownloads;
                    TotalWeeklyDownloads += version.WeeklyDownloads;
                }
            }
        }
        catch { }
        Platforms = platforms.ToArray();
        Versions = versions_list.OrderByDescending(i => i.Posted).ToDictionary(i => i.ID);
    }

    #endregion Public Methods

}