using System.Text;
using NWDUpdater.Models;

namespace NWDUpdater.Services;

public static class ScriptGeneratorService
{
    public static (string vbsPath, string batPath) GenerateScripts(AppSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.NwfFolder))
            throw new InvalidOperationException("NWF source folder is not set.");

        if (string.IsNullOrWhiteSpace(settings.NwdFolder))
            throw new InvalidOperationException("NWD output folder is not set.");

        if (!Directory.Exists(settings.NwfFolder))
            throw new DirectoryNotFoundException($"NWF folder not found: {settings.NwfFolder}");

        var nwfFiles = Directory.GetFiles(settings.NwfFolder, "*.nwf");
        if (nwfFiles.Length == 0)
            throw new InvalidOperationException($"No .nwf files found in:\n{settings.NwfFolder}");

        Directory.CreateDirectory(settings.NwdFolder);
        Directory.CreateDirectory(settings.ScriptsFolder);

        string vbsPath = Path.Combine(settings.ScriptsFolder, "NWDUpdater_Convert.vbs");
        string batPath = Path.Combine(settings.ScriptsFolder, "NWDUpdater_Run.bat");
        string logPath = Path.Combine(settings.ScriptsFolder, "NWDUpdater_Execution.log");

        var vbs = new StringBuilder();
        vbs.AppendLine("'===================================================");
        vbs.AppendLine("' NWD Updater - Auto-generated script");
        vbs.AppendLine($"' Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        vbs.AppendLine("' DO NOT EDIT MANUALLY - Use NWD Updater app");
        vbs.AppendLine("'===================================================");
        vbs.AppendLine();
        vbs.AppendLine("Dim navis_doc");
        vbs.AppendLine("Set navis_doc = CreateObject(\"Navisworks.Document\")");
        vbs.AppendLine("navis_doc.visible = false");
        vbs.AppendLine();

        foreach (string nwfPath in nwfFiles.OrderBy(f => f))
        {
            string baseName = Path.GetFileNameWithoutExtension(nwfPath);
            string nwdPath = Path.Combine(settings.NwdFolder, baseName + ".nwd");

            vbs.AppendLine($"' {baseName}");
            vbs.AppendLine($"navis_doc.OpenFile(\"{nwfPath}\")");
            vbs.AppendLine($"navis_doc.SaveAs(\"{nwdPath}\")");
            vbs.AppendLine();
        }

        vbs.AppendLine("Set navis_doc = Nothing");
        vbs.AppendLine("WScript.Echo \"NWD Update completed: \" & Now()");

        File.WriteAllText(vbsPath, vbs.ToString(), Encoding.Default);

        var bat = new StringBuilder();
        bat.AppendLine("@echo off");
        bat.AppendLine("REM NWD Updater - Auto-generated batch file");
        bat.AppendLine($"REM Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        bat.AppendLine($"echo NWD Update started: %date% %time% >> \"{logPath}\"");
        bat.AppendLine($"cscript //nologo \"{vbsPath}\"");
        bat.AppendLine($"echo NWD Update completed: %date% %time% >> \"{logPath}\"");

        File.WriteAllText(batPath, bat.ToString(), Encoding.Default);

        LogService.AppendEntry($"Scripts generated for {nwfFiles.Length} file(s) from {settings.NwfFolder}");

        return (vbsPath, batPath);
    }
}
