// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using CLMath;
using Newtonsoft.Json.Linq;
using Octokit;
using OpenDSM.Core.Models;
using OpenDSM.SQL;
using System.Net.Http.Json;
using System.Text;
using System.Xml.Linq;

namespace OpenDSM.Core.Handlers;

public record GitRepository(long ID, string Name);
public record GitCredentials(string Username, string Token);

public static class GitHandler
{

    #region Public Methods

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

    public async static Task CreateWebHook(GitCredentials credentials, ProductModel product)
    {
        if (CheckCredentials(credentials))
        {
            GitHubClient client = GetClient(credentials);
            Dictionary<string, string> config = new Dictionary<string, string>();
            config.Add("url", $"https://opendsm.tk/api/product/trigger-version-check?product_id={product.Id}");
            config.Add("content_type", "application/json");

            await client.Repository.Hooks.Create(credentials.Username, product.GitRepositoryName, new("opendsm", config)
            {
                Active = true,
                Events = new string[] { "release" },
            });
        }
    }

    public static GitRepository[] GetRepositories(GitCredentials credentials)
    {
        List<GitRepository> repos = new();
        if (CheckCredentials(credentials))
        {
            GitHubClient client = GetClient(credentials);
            IReadOnlyList<Repository> repositories = client.Repository.GetAllForCurrent().Result;
            foreach (Repository repository in repositories)
            {
                repos.Add(new(repository.Id, repository.FullName));
            }
        }

        return repos.ToArray();
    }

    public static bool GetVersionFromID(string git_repo, int git_version_id, int product_id, GitCredentials credentials, out VersionModel version)
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
                log.Error("Unable to upload release assets", e.Message, e.StackTrace);
            }
        }

        return false;
    }

    #endregion Public Methods

    #region Private Methods

    private static GitHubClient GetClient(GitCredentials credentials)
    {
        return new(new ProductHeaderValue(credentials.Username))
        {
            Credentials = new(credentials.Token)
        };
    }

    #endregion Private Methods

}