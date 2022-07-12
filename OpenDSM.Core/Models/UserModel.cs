using OpenDSM.SQL;

namespace OpenDSM.Core.Models;

public class UserModel
{
    public int Id { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string Token { get; private set; }
    public string GitToken { get; private set; }
    public AccountType Type { get; private set; }
    public int[] OwnedProducts { get; private set; }
    public int[] CreatedProducts { get; private set; }

    protected UserModel(int id, string username, string email, string token, AccountType type, int[] ownedProducts, int[] createdProducts)
    {
        Id = id;
        Username = username;
        Email = email;
        Token = token;
        Type = type;
        OwnedProducts = ownedProducts;
        CreatedProducts = createdProducts;
    }

    public void UpdateSetting(string name, dynamic value)
    {
        Authoriztaion.UpdateProperty(Token, name, value);
    }

    public static UserModel? GetByID(int id)
    {
        if (Authoriztaion.GetUserFromID(id, out string username, out string email, out AccountType type, out int[] products))
        {
            return new(id, username, email, "", type, products, Array.Empty<int>());
        }
        return null;
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
    public static bool TryGetUser(string username, string password, out UserModel? user)
    {
        return TryGetUser(username, password, out user, out FailedReason _);
    }

    public static bool TryGetUser(string username, string password, out UserModel? user, out FailedReason reason)
    {
        user = GetUser(username, password, out reason);
        return user != null;
    }

    public static UserModel? GetUser(string username, string password, out FailedReason reason)
    {
        if (Authoriztaion.Login(username, password, out reason, out AccountType type, out int id, out string email, out string uname, out string token, out int[] products))
        {
            return new(id, uname, email, token, type, products, Array.Empty<int>());
        }
        return null;
    }
}