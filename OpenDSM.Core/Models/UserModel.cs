using OpenDSM.SQL;

namespace OpenDSM.Core.Models;

public class UserModel
{
    #region Protected Constructors

    protected UserModel(int id, string username, string email, string token, AccountType type, int[] ownedProducts, int[] createdProducts)
    {
        Id = id;
        Username = username;
        Email = email;
        Token = token;
        Type = type;
        OwnedProducts = ownedProducts;
        CreatedProducts = createdProducts;
        ProfileImage = Path.Combine(GetUsersProfileDirectory(Id), "profile.jpg");
        ProfileBannerImage = Path.Combine(GetUsersProfileDirectory(Id), "banner.jpg");
    }

    #endregion Protected Constructors

    #region Public Properties

    public string About { get; set; }
    public int[] CreatedProducts { get; private set; }
    public string Email { get; private set; }
    public string GitToken { get; private set; }
    public int Id { get; private set; }
    public int[] OwnedProducts { get; private set; }
    public string ProfileBannerImage { get; set; }
    public string ProfileImage { get; set; }
    public string Token { get; private set; }
    public AccountType Type { get; private set; }
    public string Username { get; private set; }

    #endregion Public Properties

    #region Public Methods

    public static UserModel? GetByID(int id)
    {
        if (Authoriztaion.GetUserFromID(id, out string username, out string email, out AccountType type, out int[] products))
        {
            return new(id, username, email, "", type, products, Array.Empty<int>());
        }
        return null;
    }

    public static UserModel? GetUser(string username, string password, out FailedReason reason)
    {
        if (Authoriztaion.Login(username, password, out reason, out AccountType type, out int id, out string email, out string uname, out string token, out int[] products))
        {
            return new(id, uname, email, token, type, products, Array.Empty<int>());
        }
        return null;
    }

    public static bool TryGetUser(string username, string password, out UserModel? user)
    {
        return TryGetUser(username, password, out user, out FailedReason _);
    }

    public static bool TryGetUser(string username, string password, out UserModel? user, out FailedReason reason)
    {
        user = GetUser(username, password, out reason);
        return user != null;
    }

    public static bool TryGetUserWithToken(string email, string password, out UserModel? user)
    {
        user = null;
        if (Authoriztaion.LoginWithToken(email, password, out var reason, out AccountType type, out int id, out string r_email, out string uname, out string token, out int[] products))
        {
            user = new(id, uname, email, token, type, products, Array.Empty<int>());
            return true;
        }
        return false;
    }

    public void UpdateSetting(string name, dynamic value)
    {
        Authoriztaion.UpdateProperty(Token, name, value);
    }

    public async Task UploadImage(string base64, bool isProfile)
    {
        byte[] buffer;
        try
        {
            buffer = Convert.FromBase64String(base64);
        }
        catch (FormatException e)
        {
            log_user.Error($"Unable to convert base64 string to byte array: {base64}", e);
            return;
        }
        using (MemoryStream memStream = new(buffer))
        {
            using FileStream fs = new(isProfile ? ProfileImage : ProfileBannerImage, FileMode.OpenOrCreate, FileAccess.Write);
            await memStream.CopyToAsync(fs);
        }
        if (isProfile)
        {
            await FFmpeg.Instance.Resize(300, ProfileImage);
        }
        else
        {
            await FFmpeg.Instance.Resize(1280, ProfileBannerImage);
        }
    }

    #endregion Public Methods
}