using ChaseLabs.CLConfiguration.List;
using OpenDSM.Lib.Objects;
using CLMath;

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
        manager.Add("username", "");
        manager.Add("email", "");
        manager.Add("password", "");
        manager.Add("profile_image", "");
        manager.Add("description", "");
        manager.Add("owned_products", Array.Empty<long>());
        manager.Add("joined", DateTime.Now);
        manager.Add("last_online", DateTime.Now);
    }

    #endregion Public Constructors

    #region Properties

    public string Description { get => manager.GetConfigByKey("description").Value; set => manager.GetConfigByKey("description").Value = value; }
    public string Email { get => manager.GetConfigByKey("email").Value; set => manager.GetConfigByKey("email").Value = value; }
    public Guid ID { get; }
    public DateTime JoinedDate { get => DateTime.Parse(manager.GetConfigByKey("joined").Value); set => manager.GetConfigByKey("joined").Value = value.ToString("O"); }
    public DateTime LastOnlineDate { get => DateTime.Parse(manager.GetConfigByKey("last_online").Value); set => manager.GetConfigByKey("last_online").Value = value.ToString("O"); }
    public long[] OwnedProducts { get => manager.GetConfigByKey("owned_products").Value; set => manager.GetConfigByKey("owned_products").Value = value; }
    public string Password { get => manager.GetConfigByKey("password").Value; set => manager.GetConfigByKey("password").Value = AESMath.EncryptStringAES(value, ID.ToString()); }
    public string ProfileImage { get => manager.GetConfigByKey("profile_image").Value; set => manager.GetConfigByKey("profile_image").Value = value; }
    public string Username { get => manager.GetConfigByKey("username").Value; set => manager.GetConfigByKey("username").Value = value; }

    #endregion Properties

    #region Public Methods

    public bool CheckPassword(string password)
    {
        return password == AESMath.DecryptStringAES(Password, ID.ToString());
    }

    #endregion Public Methods
}