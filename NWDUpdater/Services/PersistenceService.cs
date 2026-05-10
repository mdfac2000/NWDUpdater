using System.Text.Json;
using NWDUpdater.Models;

namespace NWDUpdater.Services;

public static class PersistenceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // When running as a self-contained single file, BaseDirectory points to the
    // temp extraction folder — use ProcessPath so files land next to the .exe.
    private static string AppDir =>
        Path.GetDirectoryName(Environment.ProcessPath
            ?? AppDomain.CurrentDomain.BaseDirectory)
        ?? AppDomain.CurrentDomain.BaseDirectory;

    private static string ConfigFilePath =>
        Path.Combine(AppDir, "config.json");

    public static void Save(AppSettings settings)
    {
        string json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(ConfigFilePath, json);
    }

    public static AppSettings Load()
    {
        if (!File.Exists(ConfigFilePath))
        {
            return new AppSettings
            {
                ScriptsFolder = Path.Combine(AppDir, "Scripts")
            };
        }

        string json = File.ReadAllText(ConfigFilePath);
        var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();

        if (string.IsNullOrWhiteSpace(settings.ScriptsFolder))
            settings.ScriptsFolder = Path.Combine(AppDir, "Scripts");

        return settings;
    }
}
