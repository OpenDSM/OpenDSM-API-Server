using OpenDSM.Git.Model;
using OpenDSM.SQL;

namespace OpenDSM.Git.SQL;

public static class GitDB
{
    private static readonly string table = "git_users";

    public static bool CreateGitUser(GitUserModel user) =>
        Requests.Insert(
            table: table,
            new KeyValuePair<string, dynamic>[]
            {
                new("account_name", user.Username),
                new("account_token", user.Token),
            }
        );

    public static GitUserModel? GetGitUser(int user_id)
    {
        MySql.Data.MySqlClient.MySqlDataReader reader = Requests.Select(
            table: table,
            column: "*",
            where: new(new IndividualWhereClause[]
            {
                new("user_id", user_id, "=")
            }),
            limit: 1
        );
        if (reader.Read())
        {
            return new GitUserModel()
            {
                Username = reader.GetString("account_name"),
                Token = reader.GetString("account_token"),
            };
        }
        return null;
    }
}
