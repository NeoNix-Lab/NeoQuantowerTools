using System.Collections.Generic;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.Sqlite3Connections
{
    public interface ISqliteDynamicAccessService
    {
        string _connectionString { get; }

        Task CreateTableAsync(string tableName, List<ColumnInfo> columns);
        Task<int> DeleteAsync(string tableName, string keyColumn, object keyValue);
        Task DropTableAsync(string tableName);
        Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object> parameters = null);
        Task<List<ColumnInfo>> GetTableColumnsAsync(string tableName);
        Task<int> InsertAsync(string tableName, Dictionary<string, object> row);
        Task<List<string>> ListTablesAsync();
        Task<List<Dictionary<string, object>>> QueryAsync(string sql, Dictionary<string, object> parameters = null);
        Task<List<Dictionary<string, object>>> SelectAllAsync(string tableName, int? limit = null);
        Task<Dictionary<string, object>> SelectByIdAsync(string tableName, string keyColumn, object keyValue);
        void setConnectionString(string dbPath);
        Task<bool> TableExistsAsync(string tableName);
        Task<int> UpdateAsync(string tableName, string keyColumn, object keyValue, Dictionary<string, object> updates);
    }
}