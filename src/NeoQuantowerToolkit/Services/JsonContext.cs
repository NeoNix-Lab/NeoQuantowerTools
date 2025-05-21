using Neo.Quantower.Abstractions.Models;
using System.Text.Json.Serialization;

namespace Neo.Quantower.Toolkit.Services
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Config))]
    public partial class JesonsContexts : JsonSerializerContext
    {
    }
}
