// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using Octokit;
using OpenDSM.Core.Models;
using OpenDSM.SQL;

namespace OpenDSM.Core.Handlers;

/// <summary>
/// A shorthand for creating and storing git repositories
/// </summary>
/// <param name="ID">The git repository id</param>
/// <param name="Name">The git repository name</param>
/// <returns></returns>
public record GitRepository(long ID, string Name);

/// <summary>
/// A shorthand for creating and storing git credentials
/// </summary>
/// <param name="Username">The git username</param>
/// <param name="Token">The git token</param>
/// <returns></returns>
public record GitCredentials(string Username, string Token);

/// <summary>
/// Handles communications to github via the git api and Octokit
/// </summary>
public static class GitHandler
{

    #region Public Methods

    /// <summary>
    /// Checks if the credentials provided are valid
    /// </summary>
    /// <param name="credentials">The users git credentials</param>
    /// <returns></returns>
    public static bool CheckCredentials(GitCredentials credentials)
    {
        if (!string.IsNullOrWhiteSpace(credentials.Token) && !string.IsNullOrWhiteSpace(credentials.Username))
        {
            try
            {
                GitHubClient client = GetClient(credentials);
                return true;
            }
            catch
            {

            }
        }
        return false;
    }

    /// <summary>
    /// Creates a github release for the specified <see cref="ProductModel">product</see>
    /// </summary>
    /// <param name="credentials">The users credentials that the release will be published under</param>
    /// <param name="product">The product that the release will be published under</param>
    /// <param name="name">The name of the release</param>
    /// <param name="type">The type of release</param>
    /// <param name="changelog">The changelog of the release</param>
    /// <returns></returns>
    public async static Task<int> CreateRelease(GitCredentials credentials, ProductModel product, string name, ReleaseType type, string changelog)
    {
        try
        {
            GitHubClient client = GetClient(credentials);
            Release release = await client.Repository.Release.Create(credentials.Username, product.GitRepositoryName, new(name)
            {
                Name = $"{type} - {name}",
                Body = changelog,
            });
            return release.Id;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// Creates a web hook on the github repository, so when releases are updated it sends a ping to the scan endpoint
    /// </summary>
    /// <param name="credentials">The users git credentials</param>
    /// <param name="product">The product that the hook will hook into</param>
    /// <returns></returns>
    public async static Task CreateWebHook(GitCredentials credentials, ProductModel product)
    {
        if (CheckCredentials(credentials))
        {
            GitHubClient client = GetClient(credentials);

            await client.Repository.Hooks.Create(credentials.Username, product.GitRepositoryName, new("opendsm", new Dictionary<string, string>()
            {
                { "url", $"https://opendsm.tk/api/products/{product.Id}/releases/scan" },
                { "content_type", "application/json" }
            })
            {
                Active = true,
                Events = new string[] { "release" },
            });
        }
    }

    /// <summary>
    /// Gets a list of all git repositories for a specific user.
    /// </summary>
    /// <param name="credentials">The credentials for the user</param>
    /// <returns></returns>
    public static GitRepository[] GetRepositories(GitCredentials credentials)
    {
        List<GitRepository> repos = new();
        if (CheckCredentials(credentials))
        {
            GitHubClient client = GetClient(credentials);
            IReadOnlyList<Repository> repositories = client.Repository.GetAllForCurrent().Result;
            foreach (Repository repository in repositories)
            {
                repos.Add(new(repository.Id, repository.Name));
            }
        }

        return repos.ToArray();
    }

    /// <summary>
    /// Gets the version information based on the version id
    /// </summary>
    /// <param name="git_repo">The github repository name</param>
    /// <param name="git_version_id">The id of the version</param>
    /// <param name="product_id">The id of the product</param>
    /// <param name="credentials">The users credentials</param>
    /// <param name="version">The version found or null if none was found</param>
    /// <returns>If version was found or not</returns>
    public static bool GetVersionFromID(string git_repo, int git_version_id, int product_id, GitCredentials credentials, out VersionModel? version)
    {
        ReleaseType releaseType = ReleaseType.Unkown;
        List<PlatformVersion> platforms = new();
        version = null;
        if (CheckCredentials(credentials))
        {
            GitHubClient client = GetClient(credentials);
            Release release = client.Repository.Release.Get(credentials.Username, git_repo, git_version_id).Result;
            IReadOnlyList<ReleaseAsset> assets = release.Assets;
            foreach (ReleaseAsset asset in assets)
            {
                foreach (Platform platform in Enum.GetValues(typeof(Platform)))
                {
                    if (asset.Name.ToLower().Contains(platform.ToString().ToLower()))
                    {
                        platforms.Add(new(platform, asset.BrowserDownloadUrl, asset.DownloadCount, 0, asset.Size));
                        break;
                    }
                }
            }

            foreach (ReleaseType type in Enum.GetValues(typeof(ReleaseType)))
            {
                if (release.Name.ToLower().Contains(type.ToString().ToLower()))
                {
                    releaseType = type;
                    break;
                }
            }
            string version_name = release.Name.Replace(releaseType.ToString(), "").Trim().Trim('-').Trim();
            version = new(git_version_id, product_id, version_name, releaseType, platforms, release.Body, release.CreatedAt.LocalDateTime);
            return true;
        }
        return false;
    }
    /// <summary>
    /// Gets a list of all release ids
    /// </summary>
    /// <param name="repo_name">The github repository</param>
    /// <param name="credentials">The users git credentials</param>
    /// <returns></returns>
    public async static Task<long[]> GitReleases(string repo_name, GitCredentials credentials)
    {
        List<long> ids = new();
        if (CheckCredentials(credentials))
        {
            GitHubClient client = GetClient(credentials);
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll(credentials.Username, repo_name);
            foreach (Release release in releases)
            {
                ids.Add(release.Id);
            }
        }
        return ids.ToArray();
    }

    /// <summary>
    /// Checks if the repository has a readme or not
    /// </summary>
    /// <param name="RepositoryName">The name of the repository</param>
    /// <param name="credentials">The users credentials</param>
    /// <param name="ReadMe">The readme markdown as string</param>
    /// <returns></returns>
    public static bool HasReadME(string RepositoryName, GitCredentials credentials, out string ReadMe)
    {
        if (CheckCredentials(credentials))
        {
            try
            {
                GitHubClient client = GetClient(credentials);
                Readme readme = client.Repository.Content.GetReadme(credentials.Username, RepositoryName).Result;
                ReadMe = readme.Content;
                return true;
            }
            catch { }
        }
        ReadMe = "";
        return false;
    }

    /// <summary>
    /// Deletes a release version
    /// </summary>
    /// <param name="credentials">The users credentials</param>
    /// <param name="productModel">The product the release will be removed from</param>
    /// <param name="versionModel">The version to remove</param>
    /// <returns>If the task was successfull or not</returns>
    public static async Task<bool> DeleteRelease(GitCredentials credentials, ProductModel productModel, VersionModel versionModel)
    {
        if (CheckCredentials(credentials))
        {
            try
            {
                GitHubClient client = GetClient(credentials);
                await client.Repository.Release.Delete(credentials.Username, productModel.GitRepositoryName, (int)versionModel.ID);
                productModel.PopulateVersionsFromGit();
                return true;
            }
            catch
            {
            }
        }
        return false;
    }

    /// <summary>
    /// Updates the release version with specified information
    /// </summary>
    /// <param name="credentials">The users credentials</param>
    /// <param name="product">The product the release is under</param>
    /// <param name="version">The release version</param>
    /// <param name="name">The updated name for the release</param>
    /// <param name="type">The release type</param>
    /// <param name="changelog">The changelog markdown string</param>
    /// <returns>If the task was successfull or not</returns>
    public static async Task<bool> UpdateRelease(GitCredentials credentials, ProductModel product, VersionModel version, string name, ReleaseType type, string changelog)
    {

        if (CheckCredentials(credentials))
        {
            GitHubClient client = GetClient(credentials);
            await client.Repository.Release.Edit(credentials.Username, product.GitRepositoryName, (int)version.ID, new()
            {
                Name = $"{type} - {name}",
                TagName = name,
                Body = changelog
            });
            Versions.UpdateVersion(version.ID, product.Id, name, (byte)type, changelog);
            product.PopulateVersionsFromDB();
            return true;
        }

        return false;
    }
    /// <summary>
    /// Attempts to upload release asset
    /// </summary>
    /// <param name="credentials">The users credentials</param>
    /// <param name="file">The release asset file to be uploaded</param>
    /// <param name="product">The product the release is under</param>
    /// <param name="platform">The platform of the asset</param>
    /// <param name="release_id">the id of the release</param>
    /// <returns></returns>
    public async static Task<bool> UploadReleaseAsset(GitCredentials credentials, Stream file, ProductModel product, Platform platform, int release_id)
    {
        if (CheckCredentials(credentials))
        {
            try
            {
                GitHubClient client = GetClient(credentials);
                ReleaseAssetUpload assetUpload = new()
                {
                    FileName = $"{platform}.zip",
                    ContentType = "application/zip",
                    RawData = file
                };
                Release release = await client.Repository.Release.Get(product.User.GitUsername, product.GitRepositoryName, release_id);
                ReleaseAsset asset = await client.Repository.Release.UploadAsset(release, assetUpload);

                return true;
            }
            catch (Exception e)
            {
                log.Error("Unable to upload release assets", e.Message, e.StackTrace ?? "");
            }
        }

        return false;
    }

    #endregion Public Methods

    #region Private Methods

    /// <summary>
    /// Returns a github client
    /// </summary>
    /// <param name="credentials">The users credentials</param>
    /// <returns></returns>
    private static GitHubClient GetClient(GitCredentials credentials)
    {
        return new(new ProductHeaderValue(credentials.Username))
        {
            Credentials = new(credentials.Token)
        };
    }

    #endregion Private Methods

}