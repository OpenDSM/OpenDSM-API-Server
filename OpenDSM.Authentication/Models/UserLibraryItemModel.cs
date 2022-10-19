namespace OpenDSM.Authentication.Models;

public struct UserLibraryItemModel
{

    #region Public Properties

    public IReadOnlyCollection<AuthorizedClient> InstalledClients { get; init; }
    public DateTime LastUsed { get; init; }
    public float Price { get; init; }
    public int ProductID { get; init; }
    public DateTime Purchased { get; init; }
    public TimeSpan UseTime { get; init; }

    #endregion Public Properties

}
