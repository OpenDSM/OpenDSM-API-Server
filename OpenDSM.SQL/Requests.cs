global using static OpenDSM.SQL.Requests;
using System.Text;
using MySql.Data.MySqlClient;

namespace OpenDSM.SQL;
public record IndividualWhereClause(string Key, object Value, string @Operator, bool And = true);
public record WhereClause(IndividualWhereClause[] Clause, bool Inverse = false);
public record OrderByClause(string Column, bool Ascending = true);
public record TableItem(string Column, string DataType, int Size = -1, dynamic Default = null);
public sealed class Requests
{

    #region Public Methods

    public static bool CreateTable(string table, IReadOnlyCollection<TableItem> items)
    {
        if (TableExists(table)) return false;
        try
        {
            StringBuilder columns = new();
            foreach (TableItem item in items)
            {
                columns.Append($"{item.Column} {item.DataType}");
                if (item.Size < 0)
                {
                    columns.Append($"({item.Size})");
                }
                if (string.IsNullOrWhiteSpace(item.Default))
                {
                    if (item.Default.GetType().Equals(typeof(string)))
                        columns.Append($"DEFAULT '{item.Default}'");
                    else
                        columns.Append($"DEFAULT {item.Default}");
                }
            }
            Build(
                start: "CREATE TABLE",
                table: table,
                post_table: columns.ToString(),
                items: null,
                where: null,
                limit: -1,
                offset: -1,
                orderby: null
           ).ExecuteNonQuery();
            return true;
        }
        catch
        {
            return false;
        }
    }

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
    {
        if (instance.mysql_keywords.Contains(column.ToUpper()))
            column = $"`{column}`";
        return Build(
        start: $"SELECT {column} FROM",
        table: table,
        post_table: "",
        items: null,
        where: where,
        limit: limit,
        offset: offset,
        orderby: orderby)
        .ExecuteReader();
    }

    public static bool TableExists(string table)
    {
        using MySqlConnection conn = new(Instance.ConnectionString);
        conn.Open();
        using MySqlCommand cmd = new($"select * from `{table}` limit 1", conn);
        log.Debug(cmd.ExecuteNonQuery().ToString());
        return cmd.ExecuteNonQuery() > 0;
    }

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

    #endregion Public Methods

    #region Private Constructors

    private Requests()
    {
        string file = Path.Combine(Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location)?.FullName ?? ".", "mysql_keywords.json");
        using FileStream fs = new(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using StreamReader reader = new(fs);
        mysql_keywords = System.Text.Json.JsonSerializer.Deserialize<string[]>(reader.ReadToEnd()) ?? Array.Empty<string>();
    }

    #endregion Private Constructors

    #region Private Fields

    private static Requests instance = instance ??= new();
    private readonly IReadOnlyCollection<string> mysql_keywords;

    #endregion Private Fields

    #region Private Methods

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


        /// BUILD INSERT ITEMS
        if (items != null)
        {
            StringBuilder keys = new();
            StringBuilder values = new();
            for (int i = 0; i < items.Length; i++)
            {
                string name = items[i].Key;
                dynamic value = items[i].Value;

                keys.Append($"`{name}`");
                values.Append($"@{name.ToUpper()}_{i}");
                if (i != items.Length - 1)
                {
                    keys.Append(", ");
                    values.Append(", ");
                }

                cmd.Parameters.AddWithValue($"@{name.ToUpper()}_{i}", value);

            }
            if (items.Any())
                sql.Append($"({keys}) VALUES ({values})");
        }


        /// BUILD WHERE CLAUSE(s)
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
                s_where.Append($"`{name}` {op} @{name.ToUpper().Replace(" ", "_")}_{i}");
                if (i != clauses.Length - 1)
                {
                    s_where.Append(clauses[i].And ? " AND " : " OR ");
                }
                cmd.Parameters.AddWithValue($"@{name.ToUpper().Replace(" ", "_")}_{i}", value);
            }

            sql.Append($" {s_where}");
        }


        /// ORDER AND COUNT OF RETURNED VALUES
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


        /// BUILD SQL QUERY
        MySqlConnection conn = new(Instance.ConnectionString);
        conn.Open();
        cmd.Connection = conn;
        cmd.CommandText = sql.ToString().Trim();
        return cmd;
    }

    #endregion Private Methods

}