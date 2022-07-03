using ChaseLabs.CLConfiguration.List;
using ChaseLabs.Math;

namespace OpenDSM.Lib.Auth;

public class User
{
    #region Fields

    private ConfigManager manager;

    #endregion Fields

    #region Public Constructors

    public User(Guid id)
    {
        manager = new(Path.Combine(UsersDirectory, id.ToString(), $"{id}.json"));
        ID = id;
    }

    #endregion Public Constructors

    #region Properties

    public string Description { get => manager.GetOrCreate("description", "I am a generic human, doing generic human things, and here is my life story!").Value; set => manager.GetOrCreate("description", "I am a generic human, doing generic human things, and here is my life story!").Value = value; }
    public string Email { get => manager.GetOrCreate("email", "").Value; set => manager.GetOrCreate("email", "").Value = value; }
    public uint FeaturedProduct { get; set; }
    public string GithubToken { get => manager.GetOrCreate("git_token", "").Value; set => manager.GetOrCreate("git_token", "").Value = value; }
    public Guid ID { get; }
    public bool IsDeveloper { get => manager.GetOrCreate("is_developer", false).Value; set => manager.GetOrCreate("is_developer", false).Value = value; }
    public DateTime JoinedDate { get => DateTime.Parse(manager.GetOrCreate("joined", DateTime.Now).Value); set => manager.GetOrCreate("joined", DateTime.Now).Value = value.ToString("O"); }
    public DateTime LastOnlineDate { get => DateTime.Parse(manager.GetOrCreate("last_online", DateTime.Now).Value); set => manager.GetOrCreate("last_online", DateTime.Now).Value = value.ToString("O"); }
    public bool Confirmed { get => manager.GetOrCreate("confirmed", false).Value; set => manager.GetOrCreate("confirmed", false).Value = value; }

    public string[] LikedTags
    {
        get
        {
            try
            {
                return manager.GetOrCreate("liked_tags", Array.Empty<object>()).Value;
            }
            catch
            {
                return Array.Empty<string>();
            }
        }
        set => manager.GetOrCreate("liked_tags", Array.Empty<object>()).Value = value;
    }

    public uint[] OwnedProducts
    {
        get
        {
            try
            {
                return manager.GetOrCreate("owned_products", Array.Empty<object>()).Value;
            }
            catch (Exception e)
            {
                return Array.Empty<uint>();
            }
        }
        set => manager.GetOrCreate("owned_products", Array.Empty<object>()).Value = value;
    }

    public string Password { get => manager.GetOrCreate("password", "").Value; set => manager.GetOrCreate("password", "").Value = AESMath.EncryptStringAES(value, ID.ToString()); }
    public string ProfileImage { get => manager.GetOrCreate("profile_image", "").Value; set => manager.GetOrCreate("profile_image", "").Value = value; }
    public string ShortDescription { get => manager.GetOrCreate("short_description", "A brief summary about myself").Value; set => manager.GetOrCreate("short_description", "A brief summary about myself").Value = value; }
    public bool ShowGithub { get => manager.GetOrCreate("show_github", false).Value; set => manager.GetOrCreate("show_github", false).Value = value; }
    public string Username { get => manager.GetOrCreate("username", "").Value; set => manager.GetOrCreate("username", "").Value = value; }

    #endregion Properties

    #region Public Methods

    public bool CheckPassword(string password)
    {
        return password == AESMath.DecryptStringAES(Password, ID.ToString()) || password == Password;
    }

    #endregion Public Methods
}