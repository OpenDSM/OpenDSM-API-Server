using Octokit;
using OpenDSM.Git.Model;
using static OpenDSM.Core.Enums;

namespace OpenDSM.Git;
public record GitRelease(int id, string name, ProductReleaseType release_type);
public static class Connections
{
    /// <summary>
    /// Gets a list of all release ids
    /// </summary>
    /// <param name="repository_name"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public static async Task<IReadOnlyCollection<int>> GetReleases(string repository_name, GitUserModel user)
    {
        List<int> ids = new();
        if (TryGetClient(user.Username, user.Token, out GitHubClient client))
        {
            IReadOnlyCollection<Release> releases = await client.Repository.Release.GetAll(user.Username, repository_name);
            Parallel.ForEach(releases, release => ids.Add(release.Id));
        }
        return ids;
    }
    public static async Task<GitRelease?> GetRelease(long repository_id, int release_id, GitUserModel user)
    {
        if (TryGetClient(user.Username, user.Token, out GitHubClient client))
        {
            Release release = await client.Repository.Release.Get(repository_id, release_id);
            if (!release.Draft)
            {
                string fullname = release.Name;
                ProductReleaseType release_type = ProductReleaseType.Unkown;
                foreach (ProductReleaseType type in Enum.GetValues(typeof(ProductReleaseType)))
                {
                    if (fullname.ToLower().Contains(type.ToString().ToLower()))
                    {
                        release_type = type;
                        break;
                    }
                }
                string name = fullname.Replace(release_type.ToString(), "").Trim().Trim('-');

                return new(release.Id, name, release_type);

            }
        }
        return null;
    }
    /// <summary>
    /// Gets a list of users repositories
    /// </summary>
    /// <param name="user">The requesting user</param>
    /// <returns></returns>
    public static async Task<IReadOnlyCollection<Model.GitRepository>> GetRepositories(GitUserModel user)
    {
        List<Model.GitRepository> repos = new();
        if (TryGetClient(user.Username, user.Token, out GitHubClient client))
        {
            IReadOnlyList<Octokit.Repository> remote = await client.Repository.GetAllForCurrent();
            Parallel.ForEach(remote, r => repos.Add(new()
            {
                ID = r.Id,
                Name = r.Name,
                Created = r.CreatedAt.UtcDateTime,
                LastUpdated = r.UpdatedAt.UtcDateTime,
                About = client.Repository.Content.GetReadme(r.Id).Result.Content
            }));
        }
        return repos;
    }

    /// <summary>
    /// Attempts to get a valid client based on username and token
    /// </summary>
    /// <param name="username">The git username</param>
    /// <param name="token">The git token</param>
    /// <param name="client">The outputed GitHubClient or null</param>
    /// <returns>If the client was created successfully</returns>
    private static bool TryGetClient(string username, string token, out GitHubClient client)
    {
        if (!(string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(token)))
        {
            try
            {
                client = new GitHubClient(new ProductHeaderValue(username))
                {
                    Credentials = new(token)
                };
                return true;
            }
            catch { }
        }
        client = null;
        return false;
    }
    public static bool IsValidCredentials(GitUserModel user) => TryGetClient(user.Username, user.Token, out _);
}
