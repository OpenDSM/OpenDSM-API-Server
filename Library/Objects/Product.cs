using ChaseLabs.CLConfiguration.List;
using OpenDSM.Lib.Auth;

namespace OpenDSM.Lib.Objects;

public class Product
{
    #region Fields

    private ConfigManager manager;

    #endregion Fields

    #region Public Constructors

    public Product(long id, User owner, string name, string description, string repo_url, bool isExperimental, bool showGithub, string[] keywords, string[] tags, params Version[] versions)
    {
        manager = new(Path.Combine(ProductsDirectory, $"{id}.json"));
        Init();

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

    public Product(long id)
    {
        manager = new(Path.Combine(ProductsDirectory, $"{id}.json"));
        Init();
    }

    #endregion Public Constructors

    #region Properties

    public string Banner { get => manager.GetConfigByKey("banner_url").Value; set => manager.GetConfigByKey("banner_url").Value = value; }

    public string Description { get => manager.GetConfigByKey("description").Value; set => manager.GetConfigByKey("description").Value = value; }

    public string Icon { get => manager.GetConfigByKey("icon_url").Value; set => manager.GetConfigByKey("icon_url").Value = value; }

    public long ID { get => manager.GetConfigByKey("id").Value; set => manager.GetConfigByKey("id").Value = value; }

    public bool IsExperimental { get => manager.GetConfigByKey("is_experimental").Value; set => manager.GetConfigByKey("is_experimental").Value = value; }

    public string[] Keywords { get => manager.GetConfigByKey("keywords").Value; set => manager.GetConfigByKey("keywords").Value = value; }

    public string Name { get => manager.GetConfigByKey("name").Value; set => manager.GetConfigByKey("name").Value = value; }

    public User Owner { get => AccountManagement.Instance.GetUser(manager.GetConfigByKey("owner").Value); set => manager.GetConfigByKey("owner").Value = value.Username; }

    public float Price { get => manager.GetConfigByKey("price").Value; set => manager.GetConfigByKey("price").Value = value; }

    public string Repository_URL { get => manager.GetConfigByKey("repository_url").Value; set => manager.GetConfigByKey("repository_url").Value = value; }

    public Review[] Reviews { get => manager.GetConfigByKey("reviews").Value; set => manager.GetConfigByKey("reviews").Value = value; }

    public bool ShowGithub { get => manager.GetConfigByKey("show_github").Value; set => manager.GetConfigByKey("show_github").Value = value; }

    public string[] Tags { get => manager.GetConfigByKey("tags").Value; set => manager.GetConfigByKey("tags").Value = value; }

    public Version[] Versions { get => manager.GetConfigByKey("versions").Value; set => manager.GetConfigByKey("versions").Value = value; }

    public string YoutubeVideo { get => manager.GetConfigByKey("youtube_video").Value; set => manager.GetConfigByKey("youtube_video").Value = value; }

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
        list.Add(new(Versions.Length + 1, name, type, supported_os));
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

    #region Private Methods

    private void Init()
    {
        manager.Add("id", -1);
        manager.Add("name", "");
        manager.Add("description", "");
        manager.Add("owner", "");
        manager.Add("repository_url", "");
        manager.Add("icon_url", "");
        manager.Add("tags", Array.Empty<string>());
        manager.Add("keywords", Array.Empty<string>());
        manager.Add("show_github", false);
        manager.Add("is_experimental", true);
        manager.Add("price", 0f);
        manager.Add("versions", Array.Empty<string>());
        manager.Add("reviews", Array.Empty<object>());
    }

    #endregion Private Methods
}