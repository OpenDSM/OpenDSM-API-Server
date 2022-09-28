global using static OpenDSM.SQL.Requests;
using System.Text;
using MySql.Data.MySqlClient;

namespace OpenDSM.SQL;
public record IndividualWhereClause(string Key, object Value, string @Operator, bool And = true);
public record WhereClause(IndividualWhereClause[] Clause, bool Inverse = false);
public record OrderByClause(string Column, bool Ascending = true);
public static class Requests
{

    /// <summary>
    /// Builds and executes a select query.
    /// </summary>
    /// <param name="table">The table to select from</param>
    /// <param name="columns">The columns to return</param>
    /// <param name="where">Any conditions the query should abide by</param>
    /// <param name="limit">The max number of results</param>
    /// <param name="offset">The offset of the results, leave -1 for no offset</param>
    /// <param name="orderby">The order the results should be in, or null for default</param>
    /// <returns>The resulting data reader</returns>
    public static MySqlDataReader Select(string table, string[] columns, WhereClause? where = null, int limit = -1, int offset = -1, OrderByClause? orderby = null)
    => Select(table, string.Join(',', columns), where, limit, offset, orderby);

    /// <summary>
    /// Builds and executes a select query.
    /// </summary>
    /// <param name="table">The table to select from</param>
    /// <param name="columns">The column to return, or use '*' for all columns</param>
    /// <param name="where">Any conditions the query should abide by</param>
    /// <param name="limit">The max number of results, leave -1 for unlimited</param>
    /// <param name="offset">The offset of the results, leave -1 for no offset</param>
    /// <param name="orderby">The order the results should be in, or null for default</param>
    /// <returns>The resulting data reader</returns>
    public static MySqlDataReader Select(string table, string column, WhereClause? where = null, int limit = -1, int offset = -1, OrderByClause? orderby = null)
        => Build(
        start: $"SELECT {column} FROM",
        table: table,
        post_table: "",
        items: null,
        where: where,
        limit: limit,
        offset: offset,
        orderby: orderby)
        .ExecuteReader();
    /// <summary>
    /// Builds and executes a insert query.
    /// </summary>
    /// <param name="table">The table to insert into</param>
    /// <param name="items">The items to insert</param>
    /// <returns>If the action was successfull or not</returns>
    public static bool Insert(string table, KeyValuePair<string, dynamic>[] items)
        => Build(
        start: $"INSERT INTO",
        table: table,
        post_table: "",
        items: items,
        where: null,
        limit: -1,
        offset: -1,
        orderby: null)
        .ExecuteNonQuery() > 0;
    /// <summary>
    /// Builds and executes an update query.
    /// </summary>
    /// <param name="table">The table to execute the query on</param>
    /// <param name="items">The items to update</param>
    /// <param name="where">Any conditions the query should abide by</param>
    /// <param name="limit">The max number of columns to effect, leave -1 for unlimited</param>
    /// <returns></returns>
    public static bool Update(string table, KeyValuePair<string, dynamic>[] items, WhereClause? where = null, int limit = -1)
        => Build(
        start: $"UPDATE",
        table: table,
        post_table: "SET",
        items: items,
        where: where,
        limit: limit,
        offset: -1,
        orderby: null)
        .ExecuteNonQuery() > 0;

    /// <summary>
    /// Builds and executes a delete query
    /// </summary>
    /// <param name="table">The table to execute query on</param>
    /// <param name="where">Any conditions the query should abide by</param>
    /// <param name="limit">The max number of columns to delete, leave -1 for unlimited</param>
    /// <param name="orderby">The order the results should be in, or null for default</param>
    /// <returns>If the action was successfull</returns>
    public static bool Delete(string table, WhereClause? where, int limit = -1, OrderByClause? orderby = null)
        => Build(
        start: $"DELETE FROM",
        table: table,
        post_table: "",
        items: null,
        where: where,
        limit: limit,
        offset: -1,
        orderby: orderby)
        .ExecuteNonQuery() > 0;


    /// <summary>
    /// Builds a MySQL command and parameterized any parameters to avoid sql injections
    /// </summary>
    /// <param name="start">The beginning of the command</param>
    /// <param name="table">The table</param>
    /// <param name="post_table">Any post table modifiers</param>
    /// <param name="items">Any items to include in query</param>
    /// <param name="where">Any conditions the query should abide by</param>
    /// <param name="limit">The max number of items the query can effect</param>
    /// <param name="offset">The offset of the query</param>
    /// <param name="orderby">What order the results should be in</param>
    /// <returns>A MySqlCommand</returns>
    private static MySqlCommand Build(string start, string table, string post_table, KeyValuePair<string, dynamic>[]? items, WhereClause? where, int limit, int offset, OrderByClause? orderby)
    {
        MySqlCommand cmd = new();

        StringBuilder sql = new();
        sql.Append($"{start} `{table}` ");
        if (!string.IsNullOrWhiteSpace(post_table))
            sql.Append($" {post_table} ");
        if (items != null)
        {
            StringBuilder keys = new();
            StringBuilder values = new();
            for (int i = 0; i < items.Length; i++)
            {
                string name = items[i].Key;
                dynamic value = items[i].Value;

                keys.Append($"`{name}`");
                values.Append($"@{name.ToUpper()}");
                if (i != items.Length - 1)
                {
                    keys.Append(", ");
                    values.Append(", ");
                }

                cmd.Parameters.AddWithValue($"@{name.ToUpper()}", value);

            }
            if (items.Any())
                sql.Append($"({keys}) VALUES ({values})");
        }
        if (where != null)
        {
            StringBuilder s_where = new();
            IndividualWhereClause[] clauses = where.Clause;
            if (clauses.Any())
            {
                s_where.Append("WHERE ");
                if (where.Inverse)
                    s_where.Append("NOT ");
            }

            for (int i = 0; i < clauses.Length; i++)
            {
                string name = clauses[i].Key;
                object value = clauses[i].Value;
                string op = clauses[i].Operator;
                s_where.Append($"`{name}` {op} @{name.ToUpper().Replace(" ", "_")}");
                if (i != clauses.Length - 1)
                {
                    s_where.Append(clauses[i].And ? " AND " : " OR ");
                }
                cmd.Parameters.AddWithValue($"@{name.ToUpper().Replace(" ", "_")}", value);
            }

            sql.Append($" {s_where}");
        }
        if (orderby != null)
        {
            sql.Append($" ORDER BY `{orderby.Column}` {(orderby.Ascending ? "ASC" : "DESC")}");
        }
        if (limit > 0)
        {
            sql.Append($" LIMIT {limit}");
            if (offset > 0)
            {
                sql.Append($" OFFSET {limit} ROWS");
            }
        }

        MySqlConnection conn = new(Instance.ConnectionString);
        conn.Open();
        cmd.Connection = conn;
        cmd.CommandText = sql.ToString().Trim();
        return cmd;
    }

}