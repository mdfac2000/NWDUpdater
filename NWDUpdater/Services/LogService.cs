namespace NWDUpdater.Services;

public static class LogService
{
    private static readonly string LogPath = Path.Combine(
        Path.GetDirectoryName(Environment.ProcessPath
            ?? AppDomain.CurrentDomain.BaseDirectory)
        ?? AppDomain.CurrentDomain.BaseDirectory,
        "log.txt");

    private static readonly object Lock = new();

    public static void AppendEntry(string message, bool isError = false)
    {
        string tag = isError ? "ERROR" : "OK";
        string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{tag}] {message}";
        lock (Lock)
        {
            File.AppendAllText(LogPath, line + Environment.NewLine);
        }
    }

    public static List<string> GetEntries(int maxLines = 200)
    {
        if (!File.Exists(LogPath))
            return new List<string>();

        lock (Lock)
        {
            string[] allLines = File.ReadAllLines(LogPath);
            int skip = Math.Max(0, allLines.Length - maxLines);
            return allLines.Skip(skip).ToList();
        }
    }

    public static void ClearLog()
    {
        lock (Lock)
        {
            if (File.Exists(LogPath))
                File.WriteAllText(LogPath, string.Empty);
        }
    }
}
