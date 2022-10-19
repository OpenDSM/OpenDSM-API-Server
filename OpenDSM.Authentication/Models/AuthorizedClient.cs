// LFInteractive LLC. - All Rights Reserved
using System.Net;

namespace OpenDSM.Authentication.Models;

public struct AuthorizedClient
{

    #region Public Properties

    public IPAddress ClientConnectionAddress { get; init; }
    public string ClientName { get; init; }
    public DateTime LastConnected { get; set; }

    #endregion Public Properties

}