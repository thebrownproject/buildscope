using Newtonsoft.Json.Linq;
using Xunit;

namespace BuildScope.Tests;

public class ConfigTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _configPath;

    public ConfigTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "buildscope-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _configPath = Path.Combine(_tempDir, "config.json");
        Config.Reset();
    }

    public void Dispose()
    {
        Config.Reset();
        Environment.SetEnvironmentVariable("BUILDSCOPE_SUPABASE_URL", null);
        Environment.SetEnvironmentVariable("BUILDSCOPE_API_KEY", null);
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void GetSupabaseUrl_ReadsFromEnvVar()
    {
        Environment.SetEnvironmentVariable("BUILDSCOPE_SUPABASE_URL", "https://test.supabase.co");
        Assert.Equal("https://test.supabase.co", Config.GetSupabaseUrl());
    }

    [Fact]
    public void GetApiKey_ReadsFromEnvVar()
    {
        Environment.SetEnvironmentVariable("BUILDSCOPE_API_KEY", "test-key-123");
        Assert.Equal("test-key-123", Config.GetApiKey());
    }

    [Fact]
    public void GetSupabaseUrl_FallsBackToConfigJson()
    {
        Environment.SetEnvironmentVariable("BUILDSCOPE_SUPABASE_URL", null);
        var json = new JObject
        {
            ["supabaseUrl"] = "https://file.supabase.co",
            ["apiKey"] = "file-key"
        };
        File.WriteAllText(_configPath, json.ToString());

        Config.SetConfigPath(_configPath);
        Assert.Equal("https://file.supabase.co", Config.GetSupabaseUrl());
    }

    [Fact]
    public void GetApiKey_FallsBackToConfigJson()
    {
        Environment.SetEnvironmentVariable("BUILDSCOPE_API_KEY", null);
        var json = new JObject
        {
            ["supabaseUrl"] = "https://file.supabase.co",
            ["apiKey"] = "file-key"
        };
        File.WriteAllText(_configPath, json.ToString());

        Config.SetConfigPath(_configPath);
        Assert.Equal("file-key", Config.GetApiKey());
    }

    [Fact]
    public void Save_WritesConfigJson()
    {
        Config.SetConfigPath(_configPath);
        Config.Save("https://saved.supabase.co", "saved-key");

        var json = JObject.Parse(File.ReadAllText(_configPath));
        Assert.Equal("https://saved.supabase.co", json["supabaseUrl"]?.ToString());
        Assert.Equal("saved-key", json["apiKey"]?.ToString());
    }

    [Fact]
    public void Save_UpdatesCachedValues()
    {
        Config.SetConfigPath(_configPath);
        Config.Save("https://cached.supabase.co", "cached-key");

        Assert.Equal("https://cached.supabase.co", Config.GetSupabaseUrl());
        Assert.Equal("cached-key", Config.GetApiKey());
    }

    [Fact]
    public void EnvVar_TakesPrecedenceOverConfigJson()
    {
        var json = new JObject
        {
            ["supabaseUrl"] = "https://file.supabase.co",
            ["apiKey"] = "file-key"
        };
        File.WriteAllText(_configPath, json.ToString());
        Config.SetConfigPath(_configPath);

        Environment.SetEnvironmentVariable("BUILDSCOPE_SUPABASE_URL", "https://env.supabase.co");
        Environment.SetEnvironmentVariable("BUILDSCOPE_API_KEY", "env-key");

        Assert.Equal("https://env.supabase.co", Config.GetSupabaseUrl());
        Assert.Equal("env-key", Config.GetApiKey());
    }

    [Fact]
    public void GetSupabaseUrl_ReturnsNullWhenNotConfigured()
    {
        Environment.SetEnvironmentVariable("BUILDSCOPE_SUPABASE_URL", null);
        Config.SetConfigPath(Path.Combine(_tempDir, "nonexistent.json"));
        Assert.Null(Config.GetSupabaseUrl());
    }
}
