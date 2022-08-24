// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using OpenDSM.Core.Handlers;
using OpenDSM.SQL;
using System.Linq;
using System.Text.Json;

namespace OpenDSM.Core.Models;
public record UserProductStat(DateTime purchased, TimeSpan activeTime, float purchasePrice);
public class UserModel
{

    #region Protected Constructors

    protected UserModel(int id, string username, string email, string token, AccountType type, bool use_git_readme, Dictionary<int, UserProductStat> ownedProducts)
    {
        Id = id;
        Username = username;
        Email = email;
        Token = token;
        Type = type;
        OwnedProducts = ownedProducts;
        CreatedProducts = Products.GetProductsByOwner(id);
        UseGitReadme = use_git_readme;
        string aboutPath = Path.Combine(GetUsersProfileDirectory(Id), "about.md");
        aboutPath = File.Exists(aboutPath) ? aboutPath : "./wwwroot/assets/md/default_about.md";
        using (FileStream fs = new(aboutPath, FileMode.Open, FileAccess.Read))
        {
            using StreamReader reader = new(fs);
            About = reader.ReadToEnd();
        }
    }

    #endregion Protected Constructors

    #region Public Properties

    public string About { get; private set; }
    public int[] CreatedProducts { get; private set; }
    public string Email { get; private set; }
    public GitCredentials GitCredentials => new(GitUsername ?? "", GitToken ?? "");
    public string GitReadme
    {
        get
        {
            if (GitHandler.HasReadME(GitUsername, GitCredentials, out string readme))
            {
                return readme;
            }
            return "";
        }
    }
    public string GitToken { get; set; }
    public string GitUsername { get; set; }
    public bool HasReadme => GitHandler.HasReadME(GitUsername, GitCredentials, out string _);
    public int Id { get; private set; }
    public bool IsDeveloperAccount => !string.IsNullOrEmpty(GitUsername) && !string.IsNullOrEmpty(GitToken) && GitCredentials != null && GitHandler.CheckCredentials(GitCredentials);
    public Dictionary<int, UserProductStat> OwnedProducts { get; private set; }
    public string ProfileBannerImage
    {
        get
        {
            return Path.Combine(GetUsersProfileDirectory(Id), "banner.jpg");
        }
        set
        {
            FileHandler.CreateImageFromBase64(value, GetUsersProfileDirectory(Id), "banner", 1280);
        }
    }

    public string ProfileImage
    {
        get
        {
            return Path.Combine(GetUsersProfileDirectory(Id), "profile.jpg");
        }
        set
        {
            FileHandler.CreateImageFromBase64(value, GetUsersProfileDirectory(Id), "profile", 300);
        }
    }

    public GitRepository[] Repositories => GitHandler.GetRepositories(GitCredentials);
    public string Token { get; private set; }
    public AccountType Type { get; private set; }
    public bool UseGitReadme { get; set; }
    public string Username { get; private set; }

    #endregion Public Properties

    #region Public Methods

    public static UserModel? GetByID(int id)
    {
        try
        {
            if (Authoriztaion.GetUserFromID(id, out string username, out string email, out AccountType type, out bool use_git_readme, out string git_username, out string git_token, out int[] _))
            {
                return new(id, username, email, "", type, use_git_readme, new())
                {
                    GitUsername = git_username,
                    GitToken = git_token,
                };
            }
        }
        catch (Exception e)
        {
            log.Error($"Unable to GetuserFromID: {id}", e.Message, e.StackTrace ?? "");
        }
        return null;
    }

    public static UserModel? GetByUsername(string username)
    {
        if (Authoriztaion.GetUserFromUsername(username, out int id, out string email, out AccountType type, out bool use_git_readme, out string git_username, out string git_token, out int[] products))
        {
            return new(id, username, email, "", type, use_git_readme, new())
            {
                GitUsername = git_username,
                GitToken = git_token,
            };
        }
        return null;
    }
    public static UserModel? GetUser(string username, string password, out FailedReason reason)
    {
        if (Authoriztaion.Login(username, password, out reason, out AccountType type, out bool use_git_readme, out int id, out string email, out string uname, out string token, out string products, out string r_git_username, out string r_git_token))
        {
            return new(id, uname, email, token, type, use_git_readme, string.IsNullOrWhiteSpace(products) ? new() : JsonSerializer.Deserialize<Dictionary<int, UserProductStat>>(products))
            {
                GitToken = r_git_token,
                GitUsername = r_git_username
            };
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
        if (Authoriztaion.LoginWithToken(email, password, out var reason, out AccountType type, out bool use_git_readme, out int id, out string r_email, out string uname, out string token, out string products, out string r_git_username, out string r_git_token))
        {
            user = new(id, uname, email, token, type, use_git_readme, string.IsNullOrWhiteSpace(products) ? new() : JsonSerializer.Deserialize<Dictionary<int, UserProductStat>>(products))
            {
                GitToken = r_git_token,
                GitUsername = r_git_username
            }; ;
            return true;
        }
        return false;
    }

    public bool AddToLibrary(ProductModel product, float purchasedPrice = -1)
    {
        if (!OwnedProducts.ContainsKey(product.Id))
        {
            UserProductStat stat = new(DateTime.Now, new TimeSpan(0), purchasedPrice == -1 ? product.Price : purchasedPrice);
            OwnedProducts.Add(product.Id, stat);
            UpdateProductStat(product.Id, stat);
            return true;
        }
        return false;
    }

    public override bool Equals(object? obj)
    {
        return obj != null && obj.GetType().Equals(typeof(UserModel)) && ((UserModel)obj).Id == Id && ((UserModel)obj).Email == Email && ((UserModel)obj).Username == Username;
    }

    public void UpdateAbout(string markdown)
    {
        if (About != markdown)
        {
            string aboutPath = Path.Combine(GetUsersProfileDirectory(Id), "about.md");
            File.Delete(aboutPath);
            using (FileStream fs = new(aboutPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using StreamWriter writer = new(fs, System.Text.Encoding.UTF8);
                writer.Write(markdown);
                writer.Flush();
            }
            About = markdown;
        }
    }

    public bool UpdateProductStat(int productId, UserProductStat stat)
    {
        if (OwnedProducts.ContainsKey(productId))
        {
            OwnedProducts[productId] = stat;
            Authoriztaion.UpdateProperty(Id, Token, "owned_products", JsonSerializer.Serialize(OwnedProducts));
            return true;
        }
        return false;
    }

    public void UpdateSetting(string name, dynamic value)
    {
        Authoriztaion.UpdateProperty(Id, Token, name, value);
    }
    public object ToObject()
    {
        string profile = "", banner = "";
        using (FileStream fs = new(ProfileImage, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            using MemoryStream ms = new();
            fs.CopyTo(ms);
            profile = Convert.ToBase64String(ms.ToArray());
        }
        using (FileStream fs = new(ProfileBannerImage, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            using MemoryStream ms = new();
            fs.CopyTo(ms);
            banner = Convert.ToBase64String(ms.ToArray());
        }
        return new
        {
            Id,
            Username,
            Email,
            Token,
            About,
            CreatedProducts,
            OwnedProducts,
            HasReadme,
            git = new
            {
                useReadme = UseGitReadme,
                IsDeveloperAccount,
                readme = GitReadme,
                credentials = GitCredentials,
            },
            images = new
            {
                profile,
                banner,
            },

        };
    }

    #endregion Public Methods
}