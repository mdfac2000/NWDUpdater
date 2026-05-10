namespace NWDUpdater.Models;

public class AppSettings
{
    public string NwfFolder { get; set; } = string.Empty;
    public string NwdFolder { get; set; } = string.Empty;
    public string ScheduleTime { get; set; } = "01:00";
    public string TaskName { get; set; } = "NWDUpdater_AutoRun";
    public string ScriptsFolder { get; set; } = string.Empty;
    public string Theme { get; set; } = "Dark";
}
