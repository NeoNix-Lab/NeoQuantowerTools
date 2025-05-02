// Neo.Quantower.Toolkit.PipeDispatcher
// MessageEnvelope - Defines the structure used for message serialization across the pipe

namespace Neo.Quantower.Toolkit.PipeDispatcher
{
    /// <summary>
    /// Represents a serialized message with type metadata.
    /// </summary>
    public class MessageEnvelope
    {
        /// <summary>
        /// The assembly-qualified name of the message type.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// The JSON serialized payload of the message.
        /// </summary>
        public string JsonPayload { get; set; }
    }
}
