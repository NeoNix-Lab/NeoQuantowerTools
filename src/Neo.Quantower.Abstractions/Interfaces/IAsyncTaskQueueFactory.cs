using Neo.Quantower.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Quantower.Abstractions.Interfaces
{
    internal interface IAsyncTaskQueueFactory
    {
        AsyncTaskQueue Create(string clientName);
    }
}
