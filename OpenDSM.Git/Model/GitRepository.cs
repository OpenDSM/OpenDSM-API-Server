namespace OpenDSM.Git.Model;

public struct GitRepository
{
    public long ID { get; init; }
    public string Name { get; init; }
    public string About { get; init; }
    public DateTime LastUpdated { get; init; }
    public DateTime Created { get; init; }
}
