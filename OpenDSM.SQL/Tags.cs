// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using MySql.Data.MySqlClient;

namespace OpenDSM.SQL;

public static class Tags
{

    #region Public Methods

    public static Dictionary<int, string> GetTags()
    {
        Dictionary<int, string> tags = new();

        using MySqlDataReader reader = Select(
            table: "tags",
            column: "*"
        );
        while (reader.Read())
        {
            tags.Add(reader.GetInt32("id"), reader.GetString("name"));
        }

        return tags;
    }

    #endregion Public Methods

}