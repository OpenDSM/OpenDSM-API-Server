// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json.Linq;
using OpenDSM.Core.Handlers;
using OpenDSM.SQL;

namespace OpenDSM.Core.Models;
public record UserProductStat(DateTime purchased, TimeSpan activeTime, float purchasePrice);
public class UserModel
{

    #region Protected Constructors

    internal UserModel(int id, string username, string email, string token, AccountType type, bool use_git_readme, Dictionary<int, UserProductStat> ownedProducts)
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
        using FileStream fs = new(aboutPath, FileMode.Open, FileAccess.Read);
        using StreamReader reader = new(fs);
        About = reader.ReadToEnd();
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
    public string BannerImage
    {
        get
        {
            string path = Path.Combine(GetUsersProfileDirectory(Id), "banner.jpg");
            if (!File.Exists(path))
                path = Path.Combine(wwwroot, "assets", "images", "missing-banner.jpg");
            return path;
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
            string path = Path.Combine(GetUsersProfileDirectory(Id), "profile.jpg");
            if (!File.Exists(path))
                path = Path.Combine(wwwroot, "assets", "images", "missing-profile-image.svg");
            return path;
        }
        set
        {
            FileHandler.CreateImageFromBase64(value, GetUsersProfileDirectory(Id), "profile", 300);
        }
    }

    public bool HasProfileImage => File.Exists(Path.Combine(GetUsersProfileDirectory(Id), "profile.jpg"));
    public bool HasBannerImage => File.Exists(Path.Combine(GetUsersProfileDirectory(Id), "banner.jpg"));

    public GitRepository[] Repositories => GitHandler.GetRepositories(GitCredentials);
    public string Token { get; private set; }
    public AccountType Type { get; private set; }
    public bool UseGitReadme { get; set; }
    public string Username { get; private set; }

    #endregion Public Properties

    #region Public Methods
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
            Authorization.UpdateProperty(Id, Token, "owned_products", JsonSerializer.Serialize(OwnedProducts));
            return true;
        }
        return false;
    }

    public void UpdateSetting(string name, dynamic value)
    {
        Authorization.UpdateProperty(Id, Token, name, value);
    }
    public object ToObject(bool includeImages = false)
    {
        string profile = "", banner = "";
        if (includeImages)
        {
            using (FileStream fs = new(ProfileImage, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using MemoryStream ms = new();
                fs.CopyTo(ms);
                profile = Convert.ToBase64String(ms.ToArray());
            }
            using (FileStream fs = new(BannerImage, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using MemoryStream ms = new();
                fs.CopyTo(ms);
                banner = Convert.ToBase64String(ms.ToArray());
            }
        }
        return new
        {
            Id,
            Username,
            Email,
            Token,
            About = UseGitReadme ? GitReadme : About,
            CreatedProducts,
            OwnedProducts,
            git = new
            {
                HasGitReadme = HasReadme,
                useReadme = UseGitReadme,
                IsDeveloperAccount,
            },
            images = new
            {
                profile = new
                {
                    base64 = profile,
                    path = $"/api/images/user/{Id}/profile",
                    mime = new FileExtensionContentTypeProvider().TryGetContentType(ProfileImage, out var contentType) ? contentType : "image/png"
                },
                banner = new
                {
                    base64 = banner,
                    path = $"/api/images/user/{Id}/banner",
                    mime = new FileExtensionContentTypeProvider().TryGetContentType(BannerImage, out contentType) ? contentType : "image/png"
                },
            },

        };
    }

    #endregion Public Methods
}