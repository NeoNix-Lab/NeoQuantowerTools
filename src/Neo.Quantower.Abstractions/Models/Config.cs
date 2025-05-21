using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Quantower.Abstractions.Models
{

    //📝 TODO: [Annida]

    public class Config
    {
        required public string DataPath { get; init; }
        required public string LogsPath { get; init; }
        required public string ModelsPath { get; init; }
        required public string UiPath { get; init; }
        required public string TraddePath { get; init; }
        required public string TrainPath { get; init; }
        public string SocketHost { get; init; } = "127.0.0.1";
        public int SocketPort { get; init; } = 5000;
    }
}
