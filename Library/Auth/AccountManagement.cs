using CLMath;

namespace OpenDSM.Lib.Auth;

public class AccountManagement
{
    #region Fields

    public static AccountManagement Instance = Instance ??= new();

    private Dictionary<string, User> _users;

    #endregion Fields

    #region Protected Constructors

    protected AccountManagement()
    {
        _users = new();
    }

    #endregion Protected Constructors

    #region Public Methods

    public User CreateUser(string email, string username, string password)
    {
        User user = new(Guid.NewGuid())
        {
            Email = email,
            Username = username,
            Password = password
        };
        _users.Add(username, user);
        return user;
    }

    public User? GetUser(string username)
    {
        return _users[username];
    }

    public void Load()
    {
        string[] dirs = Directory.GetDirectories(UsersDirectory);
        Parallel.ForEach(dirs, dir =>
        {
            if (Guid.TryParse(new DirectoryInfo(dir).Name, out Guid userId))
            {
                User user = new(userId);
                _users.Add(user.Username, user);
            }
        });
    }

    public bool TryAttemptLogin(string username, string password, out User user, out string FailedReason)
    {
        user = null;
        if (_users.ContainsKey(username))
        {
            user = _users[username];
            FailedReason = "";
            if (user != null)
            {
                if (user.CheckPassword(password))
                {
                    user.LastOnlineDate = DateTime.Now;
                    return true;
                }
                else
                {
                    FailedReason = "Password was Incorrect";
                    return false;
                }
            }
        }
        FailedReason = "Could NOT find user with specified Username/Email";
        return false;
    }

    public bool TryAttemptLogin(string username, string token, out User user)
    {
        user = null;
        if (_users.ContainsKey(username))
            user = _users[username];
        return user != null && user.Password == token;
    }

    #endregion Public Methods
}