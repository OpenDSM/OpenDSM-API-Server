// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using Newtonsoft.Json.Linq;
using OpenDSM.Core.Models;
using OpenDSM.SQL;
using System.Net.Http.Json;

namespace OpenDSM.Core.Handlers;

public record GitRepository(int ID, string Name);
public record GitCredentials(string Username, string Token);

public static class GitHandler
{
    #region Public Methods

    public static bool CheckCredentials(GitCredentials credentials)
    {
        if (!string.IsNullOrWhiteSpace(credentials.Token) && !string.IsNullOrWhiteSpace(credentials.Username))
        {
            using HttpClient client = GetClient(credentials);
            using HttpResponseMessage response = client.GetAsync($"https://api.github.com/users/{credentials.Username}/repos").Result;
            if (response.IsSuccessStatusCode)
            {
                return response.Content != null;
            }
        }
        return false;
    }

    public static GitRepository[] GetRepositories(GitCredentials credentials)
    {
        List<GitRepository> repos = new();
        if (CheckCredentials(credentials))
        {
            using HttpClient client = GetClient(credentials);
            using HttpResponseMessage response = client.GetAsync($"https://api.github.com/users/{credentials.Username}/repos").Result;
            if (response.IsSuccessStatusCode)
            {
                if (response.Content != null)
                {
                    JArray jArray = JArray.Parse(response.Content.ReadAsStringAsync().Result);
                    foreach (JToken token in jArray)
                    {
                        JObject jObject = JObject.FromObject(token);
                        if (int.TryParse(jObject["id"].ToString(), out int id))
                        {
                            repos.Add(new(id, jObject["name"].ToString()));
                        }
                    }
                }
            }
        }

        return repos.ToArray();
    }

    public static bool GetVersionFromID(string git_repo, int git_version_id, int product_id, GitCredentials credentials, out VersionModel version)
    {
        ReleaseType releaseType = ReleaseType.Unkown;
        string version_name = "";
        List<PlatformVersion> platforms = new();
        version = null;
        if (CheckCredentials(credentials))
        {
            using HttpClient client = GetClient(credentials);
            using HttpResponseMessage response = client.GetAsync($"https://api.github.com/repos/{credentials.Username}/{git_repo}/releases/{git_version_id}").Result;
            if (response.IsSuccessStatusCode)
            {
                try
                {

                    JObject json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    if (json != null)
                    {
                        version_name = (string)json["name"];
                        foreach (ReleaseType type in Enum.GetValues(typeof(ReleaseType)))
                        {
                            if (version_name.ToLower().Contains(type.ToString().ToLower()))
                            {
                                releaseType = type;
                                break;
                            }
                        }
                        version_name = version_name.Replace(releaseType.ToString(), "").Trim().Trim('-').Trim(); // 'ReleaseType - Version_Name'  -> ' - Version_Name' -> 'Version_Name'
                        if (json["assets"] != null)
                        {
                            JArray assets = (JArray)json["assets"];
                            foreach (JToken token in assets)
                            {
                                JObject asset = (JObject)token;
                                string name = (string)asset["name"];
                                foreach (Platform platform in Enum.GetValues(typeof(Platform)))
                                {
                                    if (name.ToLower().Contains(platform.ToString().ToLower()))
                                    {
                                        if (asset["browser_download_url"] != null)
                                        {
                                            platforms.Add(new(platform, (string)asset["browser_download_url"], (int)(asset["download_count"] ?? 0), 0, (long)(asset["size"] ?? 0)));
                                            break;
                                        }
                                    }
                                }
                            }
                            version = new((long)json["id"], product_id, version_name, releaseType, platforms, (string)(json["body"] ?? ""));
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error($"Unable to GetVersionFromID: {git_version_id}", e.Message, e.StackTrace);
                }
            }
        }
        return false;
    }

    public static int[] GitReleases(string repo_name, GitCredentials credentials)
    {
        List<int> ids = new();
        if (CheckCredentials(credentials))
        {
            using HttpClient client = GetClient(credentials);
            using HttpResponseMessage response = client.GetAsync($"https://api.github.com/repos/{credentials.Username}/{repo_name}/releases").Result;
            if (response.IsSuccessStatusCode)
            {
                JArray array = JArray.Parse(response.Content.ReadAsStringAsync().Result);
                foreach (JToken token in array)
                {
                    JObject json = (JObject)token;
                    ids.Add((int)json["id"]);
                }
            }
        }
        return ids.ToArray();
    }

    public static bool HasReadME(string RepositoryName, GitCredentials credentials, out string ReadMe)
    {
        if (CheckCredentials(credentials))
        {
            using HttpClient client = GetClient(credentials);
            using HttpResponseMessage response = client.GetAsync($"https://api.github.com/repos/{credentials.Username}/{RepositoryName}/contents/").Result;
            if (response.IsSuccessStatusCode)
            {
                JArray jarray = JArray.Parse(response.Content.ReadAsStringAsync().Result);
                foreach (var item in jarray)
                {
                    JObject obj = (JObject)item;
                    if (obj != null && obj["name"] != null)
                    {
                        if (obj["name"].ToString().ToLower().Equals("readme.md") && obj["download_url"] != null)
                        {
                            string url = obj["download_url"].ToString();
                            using HttpClient c = GetClient(credentials);
                            using HttpResponseMessage msg = c.GetAsync(url).Result;
                            if (msg.IsSuccessStatusCode)
                            {
                                if (msg.Content != null)
                                {
                                    ReadMe = msg.Content.ReadAsStringAsync().Result;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        }
        ReadMe = "";
        return false;
    }

    public static void CreateWebHook(GitCredentials credentials, ProductModel product)
    {
        if (CheckCredentials(credentials))
        {
            using HttpClient client = GetClient(credentials);
            using HttpRequestMessage message = new(HttpMethod.Post, $"https://api.github.com/repos/{credentials.Username}/{product.GitRepositoryName}/hooks");
            message.Content = JsonContent.Create(new
            {
                active = true,
                events = new string[] { "release" },
                config = new
                {
                    url = $"https://opendsm.tk/api/product/trigger-version-check?product_id={product.Id}",
                    content_type = "application/json"
                }
            });
            using HttpResponseMessage response = client.Send(message);
        }
    }

    #endregion Public Methods

    #region Private Methods

    private static HttpClient GetClient(GitCredentials credentials)
    {
        HttpClient client = new();
        client.DefaultRequestHeaders.Add("User-Agent", ApplicationName);
        client.DefaultRequestHeaders.Add("Authorization", $"token {credentials.Token}");
        return client;
    }

    #endregion Private Methods
}