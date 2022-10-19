using OpenDSM.Authentication.Models;
using OpenDSM.Authentication.SQL;
using System.Net;

namespace OpenDSM.Authentication.Collections;

public sealed class AuthorizedClientCollection
{
    #region Public Fields

    public static readonly int MAX_AUTHORIZED_CLIENTS = 5;

    #endregion Public Fields

    #region Public Properties

    private ICollection<AuthorizedClient> _clients { get; set; }
    public IReadOnlyCollection<AuthorizedClient> Clients => (IReadOnlyCollection<AuthorizedClient>)_clients;

    #endregion Public Properties

    #region Internal Constructors

    internal AuthorizedClientCollection(int user_id)
    {
        _clients = (ICollection<AuthorizedClient>)ClientsDB.GetClients(user_id);
    }
    ~AuthorizedClientCollection()
    {
        log.Debug("Deconstructing Authorized User Collection");
        /// TODO: Add update database code here
    }
    public void Add(string Name, IPAddress connection)
    {
        if (_clients.Any(c => c.ClientName.Equals(Name) && c.ClientConnectionAddress.Equals(connection.ToString())))
        {
            AuthorizedClient client = _clients.FirstOrDefault(c => c.ClientName.Equals(Name) && c.ClientConnectionAddress.Equals(connection.ToString()));
            client.LastConnected = DateTime.Now;
        }
        else
        {
            _clients.Add(new() { ClientName = Name, ClientConnectionAddress = connection, LastConnected = DateTime.Now });
        }
        _clients = (ICollection<AuthorizedClient>)_clients.OrderBy(i => i.LastConnected);
        if (_clients.Count > MAX_AUTHORIZED_CLIENTS)
        {
            _clients = (ICollection<AuthorizedClient>)_clients.Take(MAX_AUTHORIZED_CLIENTS);
        }
        log.Debug("Updating Collection");
        /// TODO: Add update database code here
    }
    public bool Contains(string name, IPAddress connection) => _clients.Any(i => i.ClientName.Equals(name) && i.ClientConnectionAddress == connection);

    #endregion Internal Constructors

}
