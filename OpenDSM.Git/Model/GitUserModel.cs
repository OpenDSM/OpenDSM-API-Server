namespace OpenDSM.Git.Model;

public struct GitUserModel
{
    #region Public Properties

    public IReadOnlyCollection<GitRepository> Repositories => Connections.GetRepositories(this).Result;
    public string Token { get; init; }
    public string Username { get; init; }

    #endregion Public Properties
}
