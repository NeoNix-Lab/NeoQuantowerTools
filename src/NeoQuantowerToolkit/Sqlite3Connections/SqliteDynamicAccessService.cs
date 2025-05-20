using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.Sqlite3Connections
{
    public class SqliteDynamicAccessService : ISqliteDynamicAccessService
    {
        public string _connectionString { get; private set; }

        public SqliteDynamicAccessService()
        {
        }

        public SqliteDynamicAccessService(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
        }

        public void setConnectionString(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
        }

        public async Task<List<Dictionary<string, object>>> QueryAsync(string sql, Dictionary<string, object>? parameters = null)
        {
            var result = new List<Dictionary<string, object>>();
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.AddWithValue($"@{p.Key}", p.Value);
            }

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.GetValue(i);
                result.Add(row);
            }

            return result;
        }

        public Task<List<Dictionary<string, object>>> SelectAllAsync(string tableName, int? limit = null)
        {
            var sql = $"SELECT * FROM [{tableName}]";
            if (limit.HasValue)
                sql += $" LIMIT {limit.Value}";
            return QueryAsync(sql);
        }

        public async Task<Dictionary<string, object>?> SelectByIdAsync(string tableName, string keyColumn, object keyValue)
        {
            var sql = $"SELECT * FROM [{tableName}] WHERE [{keyColumn}] = @val";
            var rows = await QueryAsync(sql, new Dictionary<string, object> { { "val", keyValue } });
            return rows.FirstOrDefault();
        }

        public async Task<List<string>> ListTablesAsync()
        {
            var result = new List<string>();
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add(reader.GetString(0));

            return result;
        }

        public async Task<List<ColumnInfo>> GetTableColumnsAsync(string tableName)
        {
            var columns = new List<ColumnInfo>();
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info([{tableName}])";

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                columns.Add(new ColumnInfo
                {
                    Name = reader.GetString(1),
                    Type = reader.GetString(2),
                    IsNullable = reader.GetInt32(3) == 0,
                    IsPrimaryKey = reader.GetInt32(5) == 1
                });
            }

            return columns;
        }

        public async Task<bool> TableExistsAsync(string tableName)
        {
            var tables = await ListTablesAsync();
            return tables.Contains(tableName);
        }

        public async Task<int> InsertAsync(string tableName, Dictionary<string, object> row)
        {
            var columns = string.Join(", ", row.Keys.Select(k => $"[{k}]"));
            var values = string.Join(", ", row.Keys.Select(k => $"@{k}"));
            var sql = $"INSERT INTO [{tableName}] ({columns}) VALUES ({values})";
            return await ExecuteNonQueryAsync(sql, row);
        }

        public async Task<int> UpdateAsync(string tableName, string keyColumn, object keyValue, Dictionary<string, object> updates)
        {
            var setClause = string.Join(", ", updates.Keys.Select(k => $"[{k}] = @{k}"));
            var sql = $"UPDATE [{tableName}] SET {setClause} WHERE [{keyColumn}] = @key";
            updates["key"] = keyValue;
            return await ExecuteNonQueryAsync(sql, updates);
        }

        public async Task<int> DeleteAsync(string tableName, string keyColumn, object keyValue)
        {
            var sql = $"DELETE FROM [{tableName}] WHERE [{keyColumn}] = @val";
            return await ExecuteNonQueryAsync(sql, new Dictionary<string, object> { { "val", keyValue } });
        }

        public async Task CreateTableAsync(string tableName, List<ColumnInfo> columns)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE IF NOT EXISTS [{tableName}] (");
            for (int i = 0; i < columns.Count; i++)
            {
                var col = columns[i];
                sb.Append($"[{col.Name}] {col.Type}");
                if (!col.IsNullable) sb.Append(" NOT NULL");
                if (col.IsPrimaryKey) sb.Append(" PRIMARY KEY");
                if (i < columns.Count - 1) sb.Append(", ");
            }
            sb.Append(");");
            await ExecuteNonQueryAsync(sb.ToString());
        }

        public async Task DropTableAsync(string tableName)
        {
            var sql = $"DROP TABLE IF EXISTS [{tableName}]";
            await ExecuteNonQueryAsync(sql);
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object>? parameters = null)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.AddWithValue($"@{p.Key}", p.Value);
            }

            return await cmd.ExecuteNonQueryAsync();
        }
    }
}
