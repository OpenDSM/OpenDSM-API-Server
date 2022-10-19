namespace OpenDSM.Git.Model;

public struct GitUserModel
{
    public string Username { get; init; }
    public string Token { get; init; }
    public IReadOnlyCollection<GitRepository> Repositories => Connections.GetRepositories(this).Result;
}
