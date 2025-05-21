using Neo.Quantower.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.Services
{
    public class JsonSettingsService : IJsonSettingsService
    {
        private readonly string _path = Path.Combine(AppContext.BaseDirectory, "config.json");

        public async Task<Config?> LoadAsync()
        {
            if (!File.Exists(_path)) return null;

            var json = await File.ReadAllTextAsync(_path);
            return JsonSerializer.Deserialize<Config>(json, JesonsContexts.Default.Config);
        }

        public async Task SaveAsync(Config config)
        {
            var json = JsonSerializer.Serialize(config, JesonsContexts.Default.Config);
            if (!File.Exists(_path))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"📄 Creating config file at {_path}");
                Console.ResetColor();

                File.Create(_path);
            }
            await File.WriteAllTextAsync(_path, json);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"📄 Config file saved at {_path}");
            Console.ResetColor();

        }

        public string? GetValue(string key)
        {
            if (!File.Exists(_path)) return null;

            using var json = File.OpenRead(_path);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty(key, out var value))
                return value.ToString();

            return null;
        }

        public void Delete()
        {
            if (File.Exists(_path))
                File.Delete(_path);
        }

        public bool Exists() => File.Exists(_path);
    }
}
