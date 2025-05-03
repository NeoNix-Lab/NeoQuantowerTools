using Neo.Quantower.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Quantower.Abstractions
{
    public interface ICustomLogger<T> where T : Enum
    {
        T LoggingLevels { get; }
        /// <summary>
        /// Logs a message with the specified log level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">The message to log.</param>
        void Log(T level, string message);

        /// <summary>
        /// Logging function to be used for logging messages.
        /// <summary"/>
        public Action<string> Logger { get; }
    }
}
