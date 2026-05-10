# NWD Updater

A Windows desktop application that automates converting Navisworks NWF files to NWD format using VBScript automation and Windows Task Scheduler.

## Requirements

- Windows 10 or later
- .NET 8.0 Runtime (desktop)
- Autodesk Navisworks (required only on the machine where the scheduled task runs, not for configuration)

## Building

```
cd NWDUpdater
dotnet build
```

Or open `NWDUpdater.sln` in Visual Studio 2022 and build.

## Running

```
cd NWDUpdater
dotnet run
```

Or run the built `NWDUpdater.exe` from the output directory.

## Usage

### 1. Add Conversion Jobs

Navigate to the **Jobs** tab and click **Add Job**. For each job, provide:
- **Job Name**: A descriptive name for the conversion
- **NWF Path**: Path to the source Navisworks NWF file
- **NWD Path**: Path where the output NWD file will be saved

### 2. Configure Schedule

Navigate to the **Schedule** tab:
1. Set the **Script Output Folder** where generated scripts will be saved
2. Set the **Task Name** for the Windows Scheduled Task
3. Choose a **time** and **frequency** (Daily or Weekly with specific days)
4. Click **Generate Scripts** to create the VBS and BAT files
5. Click **Create / Update Task** to register the scheduled task in Windows Task Scheduler

### 3. Monitor Execution

- The **Log** tab shows execution history with color-coded entries
- The status bar at the bottom shows the current task status
- Use **Run Now** on the Schedule tab to trigger immediate execution

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+N | Add new job |
| Delete | Delete last job |
| F5 | Switch to Log view |

## Files

The application creates these files next to the executable:
- `config.json` - All jobs, schedule, and settings
- `log.txt` - Execution log

Generated scripts (in the configured output folder):
- `NWDUpdater_Convert.vbs` - VBScript that opens NWF files and saves as NWD
- `NWDUpdater_Run.bat` - Batch file that runs the VBScript and logs execution
- `NWDUpdater_Execution.log` - Log from batch file execution

## Notes

- Creating/updating/deleting scheduled tasks may require administrator privileges
- The generated VBScript uses `Navisworks.Document` COM automation
- Theme preference (Dark/Light) is persisted across sessions
