using Neo.Quantower.Abstractions.Models;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.Services
{
    public interface IJsonSettingsService
    {
        void Delete();
        bool Exists();
        string GetValue(string key);
        Task<Config> LoadAsync();
        Task SaveAsync(Config config);
    }
}