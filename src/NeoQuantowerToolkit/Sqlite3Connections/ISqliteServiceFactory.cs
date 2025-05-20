namespace Neo.Quantower.Toolkit.Sqlite3Connections
{
    public interface ISqliteServiceFactory
    {
        SqliteDynamicAccessService Create(string cs);
    }
}