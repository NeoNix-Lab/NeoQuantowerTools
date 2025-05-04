using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Quantower.Abstractions.Models
{
    public enum PipeDispatcherLoggingLevels
    {
        System,
        Error,
        Success,
    }

    public enum PipeDispatcherStatus
    {
        Requested,
        Finded,
        Lost,
        Error
    }

    public enum TaskPriority
    {
        High,
        Normal,
        Low
    }

    public enum TaskResoult
    {
        Completed,
        Failed,
        Delayed,
        Reenqueued,
        Queued
    }
}
