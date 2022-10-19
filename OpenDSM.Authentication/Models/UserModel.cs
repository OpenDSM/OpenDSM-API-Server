using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using OpenDSM.Authentication.Collections;
using OpenDSM.Core.Handlers;
using OpenDSM.Git.Model;
using OpenDSM.Git.SQL;

namespace OpenDSM.Authentication.Models;

public sealed class UserModel
{
    #region Public Properties
    public int Id { get; init; }
    public string Email { get; init; }
    public string Username { get; init; }
    public bool IsDeveloperAccount { get; private set; }
    public string About { get; init; }
    public string ProfileImage { get; init; }
    public string BannerImage { get; init; }
    public bool HasProfileImage { get; init; }
    public bool HasBannerImage { get; init; }
    public UserLibraryCollection OwnedProducts { get; init; }
    public AuthorizedClientCollection Clients { get; init; }
    public APIKeyModel? API_Key { get; init; }
    public GitUserModel? GitUser { get; init; }


    internal UserModel(int id, string username, string email, string about)
    {
        Id = id;

        ProfileImage = Path.Combine(Core.Global.GetUsersProfileDirectory(Id), "images", "profile.png");
        BannerImage = Path.Combine(Core.Global.GetUsersProfileDirectory(Id), "images", "banner.jpg");
        HasProfileImage = File.Exists(ProfileImage);
        HasBannerImage = File.Exists(BannerImage);

        Username = username;
        Email = email;
        About = about;
        OwnedProducts = new(Id);
        Clients = new(Id);
        API_Key = new(this);

        GitUser = GitDB.GetGitUser(Id);
        IsDeveloperAccount = GitUser != null;
    }

    public object ToObject()
    {
        return new
        {
            Id,
            Email,
            Username,
            About,
            IsDeveloperAccount,
            images = new
            {
                Profile = new
                {
                    path = $"/images/user/{Id}/profile",
                    dimensions = FFmpegHandler.Instance.GetSize(ProfileImage),
                    type = new FileExtensionContentTypeProvider().TryGetContentType(ProfileImage, out var contentType) ? contentType : "image/png",
                },
                Banner = new
                {
                    path = $"/images/user/{Id}/banner",
                    dimensions = FFmpegHandler.Instance.GetSize(BannerImage),
                    type = new FileExtensionContentTypeProvider().TryGetContentType(BannerImage, out contentType) ? contentType : "image/png",
                }
            }
        };
    }

    public void UploadImage(bool profile, string base64)
    {
        FileHandler.CreateImageFromBase64(base64, Path.Combine(Core.Global.GetUsersProfileDirectory(Id), "images"), profile ? "profile.png" : "banner.jpg");
    }

    public bool ActivateDeveloperAccount(GitUserModel user)
    {
        if (Git.Connections.IsValidCredentials(user))
        {
            if (GitDB.CreateGitUser(user))
            {
                IsDeveloperAccount = true;
                return true;
            }
        }
        return false;
    }

    #endregion Public Properties

}
