namespace OpenDSM.Authentication.Models;

public struct APIKeyModel
{
    #region Public Constructors

    public APIKeyModel(UserModel User)
    {
        this.User = User;
        Key = "";
    }

    #endregion Public Constructors

    #region Public Fields

    public static readonly int MAX_CALLS = 50;

    #endregion Public Fields

    #region Public Properties

    public string Key { get; set; }
    public UserModel User { get; set; }

    #endregion Public Properties

    #region Public Methods

    public void GenerateKey()
    {
        Key = Guid.NewGuid().ToString().Replace("-", "");
    }

    #endregion Public Methods
}
