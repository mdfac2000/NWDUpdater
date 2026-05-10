using System.Diagnostics;
using NWDUpdater.Models;

namespace NWDUpdater.Services;

public static class TaskSchedulerService
{
    public static void CreateOrUpdateTask(AppSettings settings, string batPath)
    {
        if (!File.Exists(batPath))
            throw new FileNotFoundException($"Batch file not found: {batPath}");

        // Write a clean .ps1 file — avoids all escaping issues with -Command
        string script = $@"
$action = New-ScheduledTaskAction -Execute 'cmd.exe' -Argument '/c ""{batPath.Replace("'", "''")}""'

$trigger = New-ScheduledTaskTrigger -Daily -At '{settings.ScheduleTime}'

$settings = New-ScheduledTaskSettingsSet `
    -WakeToRun `
    -StartWhenAvailable `
    -ExecutionTimeLimit (New-TimeSpan -Hours 4) `
    -MultipleInstances IgnoreNew

$principal = New-ScheduledTaskPrincipal `
    -UserId 'SYSTEM' `
    -LogonType ServiceAccount `
    -RunLevel Highest

Register-ScheduledTask `
    -TaskName '{settings.TaskName.Replace("'", "''")}' `
    -Action $action `
    -Trigger $trigger `
    -Settings $settings `
    -Principal $principal `
    -Force | Out-Null

Write-Output 'OK'
";
        var (exit, output, error) = RunPs1(script);

        if (exit != 0 || (!output.Contains("OK") && !string.IsNullOrWhiteSpace(error)))
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(error) ? output : error);

        LogService.AppendEntry(
            $"Task \"{settings.TaskName}\" created — wakes PC daily at {settings.ScheduleTime}.");
    }

    public static void DeleteTask(string taskName)
    {
        string script = $@"
try {{
    Unregister-ScheduledTask -TaskName '{taskName.Replace("'", "''")}' -Confirm:$false
    Write-Output 'OK'
}} catch {{
    Write-Output 'OK'   # not found is fine
}}
";
        var (_, _, error) = RunPs1(script);

        if (!string.IsNullOrWhiteSpace(error) &&
            !error.Contains("cannot find", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(error);

        LogService.AppendEntry($"Task \"{taskName}\" deleted.");
    }

    public static string GetTaskStatus(string taskName)
    {
        string script = $@"
try {{
    $t = Get-ScheduledTask -TaskName '{taskName.Replace("'", "''")}' -ErrorAction Stop
    Write-Output $t.State
}} catch {{
    Write-Output 'NotFound'
}}
";
        var (_, output, _) = RunPs1(script);

        return output.Trim() switch
        {
            "Ready"    => "Ready",
            "Running"  => "Running",
            "Disabled" => "Disabled",
            _          => "Not Found"
        };
    }

    public static void RunTaskNow(string taskName)
    {
        string script = $@"
Start-ScheduledTask -TaskName '{taskName.Replace("'", "''")}'
Write-Output 'OK'
";
        var (exit, _, error) = RunPs1(script);

        if (exit != 0 && !string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException(error);

        LogService.AppendEntry($"Task \"{taskName}\" triggered manually.");
    }

    // ── helper: write script to temp .ps1, run, clean up ────────────────────
    private static (int exit, string output, string error) RunPs1(string script)
    {
        string tmp = Path.Combine(Path.GetTempPath(), $"nwdupdater_{Guid.NewGuid():N}.ps1");
        try
        {
            File.WriteAllText(tmp, script, System.Text.Encoding.UTF8);

            using var p = new Process();
            p.StartInfo = new ProcessStartInfo
            {
                FileName               = "powershell.exe",
                Arguments              = $"-NonInteractive -NoProfile -ExecutionPolicy Bypass -File \"{tmp}\"",
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true
            };
            p.Start();
            string output = p.StandardOutput.ReadToEnd().Trim();
            string error  = p.StandardError.ReadToEnd().Trim();
            p.WaitForExit(30_000);
            return (p.ExitCode, output, error);
        }
        finally
        {
            try { File.Delete(tmp); } catch { }
        }
    }
}
