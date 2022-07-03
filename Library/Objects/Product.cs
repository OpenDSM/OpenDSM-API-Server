using ChaseLabs.CLConfiguration.List;
using OpenDSM.Lib.Auth;

namespace OpenDSM.Lib.Objects;

public class Product
{
    #region Fields

    private ConfigManager manager;

    #endregion Fields

    #region Public Constructors

    public Product(uint id, User owner, string name, string description, string repo_url, bool isExperimental, bool showGithub, string[] keywords, string[] tags, params Version[] versions)
    {
        manager = new(Path.Combine(ProductsDirectory, $"{id}.json"));

        ID = id;
        Name = name;
        Description = description;
        Owner = owner;
        Repository_URL = repo_url;
        Versions = versions;
        IsExperimental = isExperimental;
        ShowGithub = showGithub;
        Keywords = keywords;
        Tags = tags;
    }

    public Product(uint id)
    {
        manager = new(Path.Combine(ProductsDirectory, $"{id}.json"));
    }

    #endregion Public Constructors

    #region Properties

    public string Banner { get => manager.GetOrCreate("banner_url", "").Value; set => manager.GetOrCreate("banner_url", "").Value = value; }
    public string Description { get => manager.GetOrCreate("description", "").Value; set => manager.GetOrCreate("description", "").Value = value; }
    public string Icon { get => manager.GetOrCreate("icon_url", "").Value; set => manager.GetOrCreate("icon_url", "").Value = value; }
    public uint ID { get => manager.GetOrCreate("id", 0).Value; set => manager.GetOrCreate("id", 0).Value = value; }
    public bool IsExperimental { get => manager.GetOrCreate("is_experimental", true).Value; set => manager.GetOrCreate("is_experimental", true).Value = value; }
    public string[] Keywords { get => manager.GetOrCreate("keywords", Array.Empty<string>()).Value; set => manager.GetOrCreate("keywords", Array.Empty<string>()).Value = value; }
    public string Name { get => manager.GetOrCreate("name", "").Value; set => manager.GetOrCreate("name", "").Value = value; }
    public User Owner { get => AccountManagement.Instance.GetUser(manager.GetOrCreate("owner", "").Value); set => manager.GetOrCreate("owner", "").Value = value.Username; }
    public float Price { get => manager.GetOrCreate("price", 0f).Value; set => manager.GetOrCreate("price", 0f).Value = value; }
    public string Repository_URL { get => manager.GetOrCreate("repository_url", "").Value; set => manager.GetOrCreate("repository_url", "").Value = value; }
    public Review[] Reviews { get => manager.GetOrCreate("reviews", Array.Empty<object>()).Value; set => manager.GetOrCreate("reviews", Array.Empty<object>()).Value = value; }
    public bool ShowGithub { get => manager.GetOrCreate("show_github", false).Value; set => manager.GetOrCreate("show_github", false).Value = value; }
    public string[] Tags { get => manager.GetOrCreate("tags", Array.Empty<string>()).Value; set => manager.GetOrCreate("tags", Array.Empty<string>()).Value = value; }
    public uint TotalDownloads { get => manager.GetOrCreate("total_downloads", 0).Value; set => manager.GetOrCreate("total_downloads", 0).Value = value; }
    public uint TotalViews { get => manager.GetOrCreate("total_views", 0).Value; set => manager.GetOrCreate("total_views", 0).Value = value; }
    public Version[] Versions { get => manager.GetOrCreate("versions", Array.Empty<object>()).Value; set => manager.GetOrCreate("versions", Array.Empty<object>()).Value = value; }
    public uint WeeklyDownloads { get => manager.GetOrCreate("weekly_downloads", 0).Value; set => manager.GetOrCreate("weekly_downloads", 0).Value = value; }
    public uint WeeklyViews { get => manager.GetOrCreate("weekly_views", 0).Value; set => manager.GetOrCreate("weekly_views", 0).Value = value; }
    public string YoutubeVideo { get => manager.GetOrCreate("youtube_video", "").Value; set => manager.GetOrCreate("youtube_video", "").Value = value; }

    #endregion Properties

    #region Public Methods

    public void AddKeywords(params string[] keywords)
    {
        List<string> list = new();
        list.AddRange(Keywords);
        list.AddRange(keywords);
        Keywords = list.ToArray();
    }

    public void AddTags(params string[] tags)
    {
        List<string> list = new();
        list.AddRange(Tags);
        list.AddRange(tags);
        Tags = list.ToArray();
    }

    public void CreateVersion(string name, ReleaseType type, params SupportedOS[] supported_os)
    {
        List<Version> list = new();
        list.AddRange(Versions);
        list.Add(new((uint)(Versions.Length + 1), name, type, supported_os));
        Versions = list.ToArray();
    }

    public Review LeaveReview(string shortDescription, string longDescription, User writer, byte star)
    {
        List<Review> list = Reviews.ToList();
        Review review = new(shortDescription, longDescription, writer, DateTime.Now, this, star);
        list.Add(review);
        Reviews = list.ToArray();
        return review;
    }

    public void RemoveKeyword(string keyword)
    {
        if (Keywords.Contains(keyword))
        {
            Keywords = Keywords.Where(i => !i.Equals(keyword)).ToArray();
        }
    }

    public void RemoveTag(string tag)
    {
        if (Tags.Contains(tag))
        {
            Tags = Tags.Where(i => !i.Equals(tag)).ToArray();
        }
    }

    #endregion Public Methods
}