using Neo.Quantower.Abstractions.Interfaces;
using System;


namespace Neo.Quantower.Toolkit.Sqlite3Connections
{
    public static class SqliteServiceFactory
    {
        public static SqliteDynamicAccessService Create(string cs)
          => new SqliteDynamicAccessService(cs);
    }
}
