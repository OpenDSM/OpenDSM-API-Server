using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDSM.Core;

public record GitRepository(int ID, string Name);
public record GitCredentials(string Username, string Token);
public static class GitHandler
{
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
                    if (obj != null && obj["name"] != null && obj["name"].Type.Equals(typeof(string)))
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

    private static HttpClient GetClient(GitCredentials credentials)
    {
        HttpClient client = new();
        client.DefaultRequestHeaders.Add("User-Agent", ApplicationName);
        client.DefaultRequestHeaders.Add("Authorization", $"token {credentials.Token}");
        return client;
    }
}
