namespace OpenDSM.Git.Model;

public struct GitRepository
{
    #region Public Properties

    public string About { get; init; }
    public DateTime Created { get; init; }
    public long ID { get; init; }
    public DateTime LastUpdated { get; init; }
    public string Name { get; init; }

    #endregion Public Properties
}
