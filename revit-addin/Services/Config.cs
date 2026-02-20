using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace BuildScope
{
    public static class Config
    {
        private static string? _supabaseUrl;
        private static string? _apiKey;
        private static string _configPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
            "config.json"
        );

        public static string? GetSupabaseUrl()
        {
            if (_supabaseUrl != null)
                return _supabaseUrl;

            var envUrl = Environment.GetEnvironmentVariable("BUILDSCOPE_SUPABASE_URL");
            if (!string.IsNullOrEmpty(envUrl))
            {
                _supabaseUrl = envUrl;
                return _supabaseUrl;
            }

            LoadFromFile();
            return _supabaseUrl;
        }

        public static string? GetApiKey()
        {
            if (_apiKey != null)
                return _apiKey;

            var envKey = Environment.GetEnvironmentVariable("BUILDSCOPE_API_KEY");
            if (!string.IsNullOrEmpty(envKey))
            {
                _apiKey = envKey;
                return _apiKey;
            }

            LoadFromFile();
            return _apiKey;
        }

        public static void Save(string supabaseUrl, string apiKey)
        {
            _supabaseUrl = supabaseUrl;
            _apiKey = apiKey;

            var json = new JObject
            {
                ["supabaseUrl"] = supabaseUrl,
                ["apiKey"] = apiKey
            };
            File.WriteAllText(_configPath, json.ToString(Formatting.Indented));
        }

        private static void LoadFromFile()
        {
            if (!File.Exists(_configPath)) return;

            try
            {
                var json = JObject.Parse(File.ReadAllText(_configPath));
                _supabaseUrl ??= json["supabaseUrl"]?.ToString();
                _apiKey ??= json["apiKey"]?.ToString();
            }
            catch { /* corrupted config, ignore */ }
        }

        // Test helpers
        internal static void SetConfigPath(string path) => _configPath = path;

        internal static void Reset()
        {
            _supabaseUrl = null;
            _apiKey = null;
            _configPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
                "config.json"
            );
        }
    }
}
