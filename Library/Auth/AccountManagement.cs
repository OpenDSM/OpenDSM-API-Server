using System.Net;
using System.Net.Mail;

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

    public bool CreateUser(string email, string username, string password, out User user, out string reason)
    {
        log.Debug($"Attempting to create user with username of \"{username}\" and email of \"{email}\"");

        reason = "";
        user = null;
        if (string.IsNullOrEmpty(email))
        {
            reason = "Email was left blank";
        }
        else if (string.IsNullOrEmpty(username))
        {
            reason = "Username was left blank";
        }
        else if (string.IsNullOrEmpty(password))
        {
            reason = "Password was left blank";
        }
        else if (_users.ContainsKey(email))
        {
            reason = $"User with email \"{email}\" already exists";
        }
        else if (_users.ContainsKey(username))
        {
            reason = $"User with username \"{username}\" already exists";
        }
        else
        {
            Guid id = Guid.NewGuid();
            user = new(id)
            {
                Email = email,
                Username = username,
                Password = password,
                Confirmed = false,
            };
            _users.Add(username, user);
            _users.Add(email, user);
            _users.Add(id.ToString(), user);
            SendConfirmationEmail(id, email, username);
            return true;
        }
        log.Warn($"Account creation failed because \"{reason}\"");
        return false;
    }

    public bool ConfirmUser(Guid id, out string reason)
    {
        if (_users.ContainsKey(id.ToString()))
        {
            User user = _users[id.ToString()];
            if (user.Confirmed)
            {
                reason = "User already confirmed";
            }
            else
            {
                reason = "User confirmed";
                user.Confirmed = true;
            }
            return true;
        }
        reason = "No such user exists";
        return false;

    }

    private static void SendConfirmationEmail(Guid id, string email, string username)
    {
        MailAddress from = new("no-reply@opendsm.net");
        MailAddress to = new(email);
        MailMessage message = new(from, to)
        {
            Subject = $"OpenDSM Account Activation",
            Body = ""
        };
        using SmtpClient client = new()
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            EnableSsl = true,
            Host = "smtp.gmail.com",
            Port = 587,
            Credentials = new NetworkCredential(from.User, ""),
        };
        client.Send(message);
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
                try
                {
                    User user = new(userId);
                    _users.Add(user.Username, user);
                    _users.Add(user.Email, user);
                    log.Debug($"Loaded user \"{userId}#{user.Username}\"");
                }
                catch (Exception e)
                {
                    log.Error($"Unable to load user with id of {userId}", e);
                }
            }
        });
    }

    public bool TryAttemptLogin(string username, string password, out User user, out string FailedReason)
    {
        log.Debug($"Attempting to log user with username of \"{username}\"");
        user = null;
        if (_users.ContainsKey(username))
        {
            user = _users[username];
            FailedReason = "";
            if (user != null)
            {
                if (user.CheckPassword(password))
                {
                    log.Debug($"Successfully logged in user with username of \"{username}\"");
                    user.LastOnlineDate = DateTime.Now;
                    return true;
                }
                else
                {
                    log.Warn($"Invalid password attempted for user \"{username}\"");
                    FailedReason = "Password was Incorrect";
                    return false;
                }
            }
            else
            {
                log.Error($"Value for key ({username}) is null");
            }
        }
        log.Warn($"No user with username of \"{username}\" exists");
        log.Warn($"Failed to log in user with username of \"{username}\"");
        FailedReason = "Could NOT find user with specified Username/Email";
        return false;
    }

    public bool TryAttemptLogin(string username, string token, out User user)
    {
        return TryAttemptLogin(username, token, out user, out string _);
    }

    #endregion Public Methods
}