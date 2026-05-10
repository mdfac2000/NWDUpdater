using System.Diagnostics;
using NWDUpdater.Models;

namespace NWDUpdater.Services;

/// <summary>
/// Creates/manages Windows Scheduled Tasks via PowerShell's ScheduledTasks module.
/// Using PowerShell (instead of schtasks.exe) lets us enable WakeToRun so the PC
/// wakes from Sleep (S3) automatically at the scheduled time.
///
/// NOTE: Wake from Hibernate (S4) or full shutdown is NOT possible via software alone.
/// The user must also enable "Allow wake timers" in Windows Power Options →
/// Sleep → Allow wake timers → Enable (or "Important wake timers only").
/// </summary>
public static class TaskSchedulerService
{
    public static void CreateOrUpdateTask(AppSettings settings, string batPath)
    {
        if (!File.Exists(batPath))
            throw new FileNotFoundException($"Batch file not found: {batPath}");

        // Escape single quotes in path for PowerShell
        string escapedBat = batPath.Replace("'", "''");
        string taskName   = settings.TaskName.Replace("'", "''");
        string time       = settings.ScheduleTime;   // "HH:mm"

        // Build PowerShell script:
        //  - Runs as SYSTEM → no interactive session required
        //  - HighestAvailable → administrator privileges
        //  - WakeToRun = $true → wakes PC from Sleep (S3)
        //  - RunOnlyIfNetworkAvailable = $false → runs regardless of network
        //  - ExecutionTimeLimit 4 hours → enough for large NWF conversions
        string ps = $@"
$action   = New-ScheduledTaskAction -Execute 'cmd.exe' -Argument '/c ""{escapedBat}""'
$trigger  = New-ScheduledTaskTrigger -Daily -At '{time}'
$settings = New-ScheduledTaskSettingsSet `
    -WakeToRun `
    -ExecutionTimeLimit (New-TimeSpan -Hours 4) `
    -RunOnlyIfNetworkAvailable $false `
    -StartWhenAvailable `
    -MultipleInstances IgnoreNew
$principal = New-ScheduledTaskPrincipal `
    -UserId 'SYSTEM' `
    -LogonType ServiceAccount `
    -RunLevel Highest
Register-ScheduledTask `
    -TaskName '{taskName}' `
    -Action $action `
    -Trigger $trigger `
    -Settings $settings `
    -Principal $principal `
    -Force | Out-Null
";

        RunPowerShell(ps, out string error);

        if (!string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException($"Failed to create task:\n{error}");

        LogService.AppendEntry($"Task \"{settings.TaskName}\" created — wakes PC daily at {time}.");
    }

    public static void DeleteTask(string taskName)
    {
        string escaped = taskName.Replace("'", "''");
        string ps = $"Unregister-ScheduledTask -TaskName '{escaped}' -Confirm:$false";

        RunPowerShell(ps, out string error);

        if (!string.IsNullOrWhiteSpace(error) &&
            !error.Contains("cannot find", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Failed to delete task:\n{error}");

        LogService.AppendEntry($"Task \"{taskName}\" deleted.");
    }

    public static string GetTaskStatus(string taskName)
    {
        string escaped = taskName.Replace("'", "''");
        string ps = $@"
try {{
    $t = Get-ScheduledTask -TaskName '{escaped}' -ErrorAction Stop
    $t.State
}} catch {{
    'NotFound'
}}
";
        RunPowerShell(ps, out _, out string output);

        string state = output.Trim();
        return state switch
        {
            "Ready"    => "Ready",
            "Running"  => "Running",
            "Disabled" => "Disabled",
            "NotFound" => "Not Found",
            _          => "Not Found"
        };
    }

    public static void RunTaskNow(string taskName)
    {
        string escaped = taskName.Replace("'", "''");
        string ps = $"Start-ScheduledTask -TaskName '{escaped}'";

        RunPowerShell(ps, out string error);

        if (!string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException($"Failed to run task:\n{error}");

        LogService.AppendEntry($"Task \"{taskName}\" triggered manually.");
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static void RunPowerShell(string script, out string error, out string output)
    {
        using var p = new Process();
        p.StartInfo = new ProcessStartInfo
        {
            FileName               = "powershell.exe",
            Arguments              = $"-NonInteractive -NoProfile -ExecutionPolicy Bypass -Command \"{EscapeArg(script)}\"",
            UseShellExecute        = false,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            CreateNoWindow         = true,
            Verb                   = "runas"   // elevate if needed
        };
        p.Start();
        output = p.StandardOutput.ReadToEnd().Trim();
        error  = p.StandardError.ReadToEnd().Trim();
        p.WaitForExit(30_000);
    }

    private static void RunPowerShell(string script, out string error)
        => RunPowerShell(script, out error, out _);

    private static string EscapeArg(string script)
        => script.Replace("\"", "\\\"").Replace(Environment.NewLine, " ");
}
