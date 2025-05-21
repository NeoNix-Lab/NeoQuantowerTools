using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Quantower.Abstractions.Interfaces
{
    public interface IModelStruct
    {
        public string? ModelPathRef { get; set; }
        public string? Name { get; set; }
        public List<string> Columns { get; set; }

        public List<string> SetCoulmns();
    }
}
