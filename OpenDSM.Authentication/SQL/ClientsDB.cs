using MySql.Data.MySqlClient;
using OpenDSM.Authentication.Collections;
using OpenDSM.Authentication.Models;
using OpenDSM.SQL;
using System.Data.Common;
using System.Net;

namespace OpenDSM.Authentication.SQL;

internal static class ClientsDB
{
    private static readonly string table = "auth_clients";
    public static IReadOnlyCollection<AuthorizedClient> GetClients(int user_id)
    {
        MySqlDataReader reader = Requests.Select(
            table: table,
            column: "*",
            where: new(new IndividualWhereClause[]
            {
                new("user_id", user_id, "=")
            }),
            limit: AuthorizedClientCollection.MAX_AUTHORIZED_CLIENTS,
            orderby: new("last_connected")
        );
        ICollection<AuthorizedClient> clients = new List<AuthorizedClient>();
        while (reader.Read())
        {
            AuthorizedClient client = new()
            {
                ClientName = reader.GetString("client_name"),
                ClientConnectionAddress = IPAddress.Parse(reader.GetString("client_address")),
                LastConnected = reader.GetDateTime("last_connected")
            };
            clients.Add(client);
        }
        return (IReadOnlyCollection<AuthorizedClient>)clients;
    }
    public static void Update(UserModel user)
    {
        IReadOnlyCollection<AuthorizedClient> remote_collection = GetClients(user.Id);
        IReadOnlyCollection<AuthorizedClient> current_collection = user.Clients.Clients;

        // Remove remote items that are no longer current
        IEnumerable<AuthorizedClient> missing = remote_collection.Where(i => !current_collection.Any(j => j.ClientName.Equals(i.ClientName) && j.ClientConnectionAddress.Equals(i.ClientConnectionAddress)));
        if (missing.Any())
        {
            foreach (AuthorizedClient client in missing)
            {
                Requests.Delete(
                    table: table,
                    where: new(new IndividualWhereClause[]
                    {
                        new("user_id", user.Id, "="),
                        new("client_name", client.ClientName, "="),
                        new("client_address", client.ClientConnectionAddress.ToString(), "="),
                    })
                );
            }
        }

        foreach (AuthorizedClient client in current_collection)
        {
            if (remote_collection.Any(i => i.ClientName.Equals(client.ClientName) && i.ClientConnectionAddress.Equals(client.ClientConnectionAddress) && !i.LastConnected.Equals(client.LastConnected)))
            {
                // Update last connected time
                Requests.Update(
                    table: table,
                    items: new KeyValuePair<string, dynamic>[]
                    {
                        new("last_connected", client.LastConnected)
                    }
                );
            }
            else
            {
                // Insert new clients
                Requests.Insert(
                    table: table,
                    items: new KeyValuePair<string, dynamic>[]
                    {
                        new("user_id", user.Id),
                        new("client_name", client.ClientName),
                        new("client_address", client.ClientConnectionAddress.ToString())
                    }
                );
            }
        }
    }
}
