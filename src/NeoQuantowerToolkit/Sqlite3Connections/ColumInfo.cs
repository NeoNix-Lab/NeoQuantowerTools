using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.Sqlite3Connections
{
    public class ColumnInfo
    {
        public string Name { get; set; } = default!;
        public string Type { get; set; } = default!;
        public bool IsPrimaryKey { get; set; } = false;
        public bool IsNullable { get; set; } = true;
    }
}
