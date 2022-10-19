namespace OpenDSM.Authentication.Models;

public struct APIKeyModel
{
    public string Key { get; set; }
    public static readonly int MAX_CALLS = 50;
    public UserModel User { get; set; }

    public APIKeyModel(UserModel User)
    {
        this.User = User;
        Key = "";
    }

    public void GenerateKey()
    {
        Key = Guid.NewGuid().ToString().Replace("-", "");
    }
}
