using NWDUpdater.Models;
using NWDUpdater.Services;

namespace NWDUpdater.Forms;

public class MainForm : Form
{
    // ── Palette (Navisworks Clash Detective style) ────────────────────────
    private static readonly Color C_HeaderBg   = Color.FromArgb(26,  31,  46);
    private static readonly Color C_ContentBg  = Color.FromArgb(248, 248, 248);
    private static readonly Color C_White      = Color.White;
    private static readonly Color C_Yellow     = Color.FromArgb(245, 196, 0);
    private static readonly Color C_Blue       = Color.FromArgb(0,   102, 204);
    private static readonly Color C_TextDark   = Color.FromArgb(25,  25,  25);
    private static readonly Color C_TextGray   = Color.FromArgb(115, 115, 115);
    private static readonly Color C_Border     = Color.FromArgb(204, 204, 204);
    private static readonly Color C_FooterLine = Color.FromArgb(220, 220, 220);
    private static readonly Color C_InputBg    = Color.FromArgb(235, 244, 255);

    private AppSettings _settings;
    private TextBox  _nwfBox     = null!;
    private Label    _nwfHint    = null!;
    private TextBox  _nwdBox     = null!;
    private DateTimePicker _time = null!;
    private Label    _statusBadge = null!;
    private Point    _dragOrigin;

    public MainForm()
    {
        _settings = PersistenceService.Load();
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        InitializeComponent();
        PollStatus();
        RefreshNwfHint();
    }

    private void InitializeComponent()
    {
        FormBorderStyle = FormBorderStyle.None;
        Size = new Size(460, 720);
        MinimumSize = Size;
        MaximumSize = Size;
        StartPosition = FormStartPosition.CenterScreen;

        // Taskbar + Alt-Tab icon
        string icoPath = Path.Combine(
            Path.GetDirectoryName(Environment.ProcessPath ?? "") ?? "",
            "kiewit.ico");
        if (File.Exists(icoPath))
            Icon = new Icon(icoPath);
        BackColor = C_ContentBg;

        // ────────────────────────────────────────────────────────
        // HEADER
        // ────────────────────────────────────────────────────────
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 64,
            BackColor = C_HeaderBg
        };

        var kBadge = new Panel
        {
            Size = new Size(34, 34),
            Location = new Point(16, 15),
            BackColor = C_Yellow
        };
        var kLbl = new Label
        {
            Text = "K",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = C_HeaderBg,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        kBadge.Controls.Add(kLbl);

        var titleLbl = new Label
        {
            Text = "NWD UPDATER",
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(62, 13)
        };

        var subtitleLbl = new Label
        {
            Text = "Navisworks Automation",
            Font = new Font("Segoe UI", 8f),
            ForeColor = Color.FromArgb(155, 165, 185),
            AutoSize = true,
            Location = new Point(63, 34)
        };

        var closeBtn = FlatButton("✕", Color.White, C_HeaderBg);
        closeBtn.Size = new Size(38, 38);
        closeBtn.Location = new Point(460 - 46, 13);
        closeBtn.Font = new Font("Segoe UI", 11f);
        closeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 65, 85);
        closeBtn.Click += (_, _) => Application.Exit();
        new ToolTip().SetToolTip(closeBtn, "Exit");

        header.Controls.AddRange(new Control[] { kBadge, titleLbl, subtitleLbl, closeBtn });

        // Allow dragging the window by the header
        foreach (Control c in new Control[] { header, kBadge, kLbl, titleLbl, subtitleLbl })
        {
            c.MouseDown += (_, e) => { if (e.Button == MouseButtons.Left) _dragOrigin = PointToScreen(e.Location); };
            c.MouseMove += (_, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    Point now = PointToScreen(e.Location);
                    Location = new Point(Location.X + now.X - _dragOrigin.X, Location.Y + now.Y - _dragOrigin.Y);
                    _dragOrigin = now;
                }
            };
        }

        // ────────────────────────────────────────────────────────
        // FOOTER
        // ────────────────────────────────────────────────────────
        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 62,
            BackColor = C_White
        };
        footer.Paint += (_, e) =>
            e.Graphics.DrawLine(new Pen(C_FooterLine), 0, 0, footer.Width, 0);

        var cancelBtn = FlatButton("Cancel", C_TextDark, C_White);
        cancelBtn.Size = new Size(90, 36);
        cancelBtn.FlatAppearance.BorderColor = C_Border;
        cancelBtn.FlatAppearance.BorderSize = 1;
        cancelBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
        cancelBtn.Font = new Font("Segoe UI", 9.5f);
        cancelBtn.Location = new Point(460 - 90 - 160 - 24, 13);
        cancelBtn.Click += (_, _) => Application.Exit();
        new ToolTip().SetToolTip(cancelBtn, "Close the application");

        var applyBtn = new Button
        {
            Text = "Apply Schedule  →",
            BackColor = C_Yellow,
            ForeColor = C_HeaderBg,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Size = new Size(160, 36),
            Cursor = Cursors.Hand
        };
        applyBtn.FlatAppearance.BorderSize = 0;
        applyBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 210, 20);
        applyBtn.Location = new Point(460 - 160 - 16, 13);
        applyBtn.Click += OnApply;
        new ToolTip().SetToolTip(applyBtn, "Generate scripts and register the Windows scheduled task");

        footer.Controls.AddRange(new Control[] { cancelBtn, applyBtn });

        // ────────────────────────────────────────────────────────
        // CONTENT  (no scroll — exact fit)
        // Budgeted height: 720 - 64 header - 62 footer = 594px
        // Measured total below: 586px  ✓
        // ────────────────────────────────────────────────────────
        var content = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = C_ContentBg
        };

        int y = 0;   // absolute y inside 'content'

        // ── NWF SOURCE FOLDER  (section=60 + gap=10 + input=30 + hint=22 + sep=16) = 138 ──
        y = AddSection(content, y, "NWF SOURCE FOLDER",
            "Select the folder containing your .nwf files, then click Load.");  // y → 70

        _nwfBox = PathBox(_settings.NwfFolder, "No folder selected…");
        _nwfBox.Location = new Point(20, y);
        _nwfBox.Width    = 348;

        var nwfLoadBtn = ActionButton("Load");
        nwfLoadBtn.Location = new Point(376, y);
        nwfLoadBtn.Click += (_, _) =>
        {
            using var d = new FolderBrowserDialog { Description = "Select folder with .nwf files" };
            if (!string.IsNullOrEmpty(_nwfBox.Text) && Directory.Exists(_nwfBox.Text))
                d.InitialDirectory = _nwfBox.Text;
            if (d.ShowDialog() == DialogResult.OK)
            {
                _nwfBox.Text        = d.SelectedPath;
                _settings.NwfFolder = d.SelectedPath;
                PersistenceService.Save(_settings);
                RefreshNwfHint();
            }
        };
        new ToolTip().SetToolTip(nwfLoadBtn, "Browse for the folder containing the .nwf files");
        y += 30;   // y → 100

        _nwfHint = new Label
        {
            Text      = "Click Load to scan for .nwf files",
            ForeColor = C_TextGray,
            Font      = new Font("Segoe UI", 8f, FontStyle.Italic),
            Location  = new Point(20, y + 4),
            AutoSize  = true
        };
        y += 22;   // y → 122
        content.Controls.AddRange(new Control[] { _nwfBox, nwfLoadBtn, _nwfHint });
        y = AddSeparator(content, y);   // y → 138

        // ── NWD OUTPUT FOLDER  (section=60+10 + input=30 + sep=16) = 116 ──
        y = AddSection(content, y, "NWD OUTPUT FOLDER",
            "The converted .nwd files will be saved to this folder.");   // y → 208

        _nwdBox = PathBox(_settings.NwdFolder, "No folder selected…");
        _nwdBox.Location = new Point(20, y);
        _nwdBox.Width    = 348;

        var nwdBrowseBtn = ActionButton("Browse");
        nwdBrowseBtn.Location = new Point(376, y);
        nwdBrowseBtn.Click += (_, _) =>
        {
            using var d = new FolderBrowserDialog { Description = "Select NWD output folder", ShowNewFolderButton = true };
            if (!string.IsNullOrEmpty(_nwdBox.Text) && Directory.Exists(_nwdBox.Text))
                d.InitialDirectory = _nwdBox.Text;
            if (d.ShowDialog() == DialogResult.OK)
            {
                _nwdBox.Text        = d.SelectedPath;
                _settings.NwdFolder = d.SelectedPath;
                PersistenceService.Save(_settings);
            }
        };
        new ToolTip().SetToolTip(nwdBrowseBtn, "Browse for the output folder");
        y += 30;   // y → 238
        content.Controls.AddRange(new Control[] { _nwdBox, nwdBrowseBtn });
        y = AddSeparator(content, y);   // y → 254

        // ── SCHEDULE  (section=60+10 + row=34 + sep=16) = 120 ──
        y = AddSection(content, y, "SCHEDULE",
            "Set the time when the conversion runs automatically each day.");   // y → 324

        var runLbl = new Label
        {
            Text      = "Run every day at",
            ForeColor = C_TextGray,
            Font      = new Font("Segoe UI", 9f),
            Location  = new Point(20, y + 6),
            AutoSize  = true
        };

        _time = new DateTimePicker
        {
            Format       = DateTimePickerFormat.Custom,
            CustomFormat = "HH:mm",
            ShowUpDown   = true,
            Font         = new Font("Segoe UI", 10f, FontStyle.Bold),
            Width        = 80,
            Height       = 26,
            Location     = new Point(158, y + 3)
        };
        if (TimeSpan.TryParse(_settings.ScheduleTime, out var ts))
            _time.Value = DateTime.Today.Add(ts);
        _time.ValueChanged += (_, _) =>
        {
            _settings.ScheduleTime = _time.Value.ToString("HH:mm");
            PersistenceService.Save(_settings);
        };
        y += 34;   // y → 358
        content.Controls.AddRange(new Control[] { runLbl, _time });
        y = AddSeparator(content, y);   // y → 374

        // ── TASK STATUS  (section=60+10 + badge=28 + buttons=36 + pad=14) = 148 ──
        y = AddSection(content, y, "TASK STATUS",
            "Current state of the scheduled task in Windows Task Scheduler.");   // y → 444

        _statusBadge = new Label
        {
            Text      = "●  Checking…",
            ForeColor = C_TextGray,
            Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Location  = new Point(20, y),
            AutoSize  = true
        };
        y += 30;   // y → 474

        var runNowBtn = new Button
        {
            Text      = "Run Now",
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 8.5f),
            ForeColor = C_TextDark,
            BackColor = C_White,
            Size      = new Size(90, 30),
            Location  = new Point(20, y),
            Cursor    = Cursors.Hand
        };
        runNowBtn.FlatAppearance.BorderColor = C_Border;
        runNowBtn.FlatAppearance.BorderSize  = 1;
        runNowBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
        runNowBtn.Click += OnRunNow;
        new ToolTip().SetToolTip(runNowBtn, "Trigger the conversion immediately");

        var removeBtn = new Button
        {
            Text      = "Remove Task",
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 8.5f),
            ForeColor = Color.FromArgb(180, 30, 30),
            BackColor = C_White,
            Size      = new Size(110, 30),
            Location  = new Point(122, y),
            Cursor    = Cursors.Hand
        };
        removeBtn.FlatAppearance.BorderColor = Color.FromArgb(180, 30, 30);
        removeBtn.FlatAppearance.BorderSize  = 1;
        removeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 235, 235);
        removeBtn.Click += OnRemoveTask;
        new ToolTip().SetToolTip(removeBtn, "Unregister the scheduled task");

        y += 36;

        // Wake notice
        var wakeNote = new Label
        {
            Text      = "⚡ Task configured to wake the PC from Sleep automatically.\r\n" +
                        "   Requires: Power Options → Sleep → Allow wake timers → Enabled",
            ForeColor = Color.FromArgb(140, 100, 0),
            BackColor = Color.FromArgb(255, 248, 220),
            Font      = new Font("Segoe UI", 7.5f),
            Location  = new Point(20, y),
            Size      = new Size(416, 36),
            Padding   = new Padding(4, 2, 4, 2)
        };
        new ToolTip().SetToolTip(wakeNote,
            "After clicking Apply Schedule, go to:\n" +
            "Control Panel → Power Options → Change plan settings\n" +
            "→ Change advanced power settings → Sleep\n" +
            "→ Allow wake timers → set to Enable");

        content.Controls.AddRange(new Control[] { _statusBadge, runNowBtn, removeBtn, wakeNote });

        Controls.Add(content);
        Controls.Add(footer);
        Controls.Add(header);

        // Poll task status every 10 seconds
        var timer = new System.Windows.Forms.Timer { Interval = 10000 };
        timer.Tick += (_, _) => PollStatus();
        timer.Start();
    }

    // ── Section builder ───────────────────────────────────────────────────

    private int AddSection(Panel parent, int y, string title, string subtitle)
    {
        // Yellow-bordered section header panel
        // section panel: 60px white with 5px yellow bar, then 10px gray gap below
        var sectionHeader = new Panel
        {
            Location  = new Point(0, y),
            Width     = 460,
            Height    = 60,
            BackColor = C_White
        };
        sectionHeader.Paint += (_, e) =>
        {
            using var b = new SolidBrush(C_Yellow);
            e.Graphics.FillRectangle(b, 0, 0, 5, sectionHeader.Height);
        };

        var titleLbl = new Label
        {
            Text      = title,
            Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = C_TextDark,
            Location  = new Point(20, 9),
            AutoSize  = true
        };

        var subLbl = new Label
        {
            Text      = subtitle,
            Font      = new Font("Segoe UI", 8f, FontStyle.Italic),
            ForeColor = C_TextGray,
            Location  = new Point(20, 32),
            Size      = new Size(430, 17)
        };

        sectionHeader.Controls.AddRange(new Control[] { titleLbl, subLbl });
        parent.Controls.Add(sectionHeader);

        // 10px gray spacer between section header and its content
        var spacer = new Panel
        {
            Location  = new Point(0, y + 60),
            Width     = 460,
            Height    = 10,
            BackColor = C_ContentBg
        };
        parent.Controls.Add(spacer);

        return y + 60 + 10;   // 70 per section header
    }

    private int AddSeparator(Panel parent, int y)
    {
        // 16px gap: 8px space, 1px line at y+8, 7px space after
        var sep = new Panel
        {
            Location  = new Point(0, y),
            Width     = 460,
            Height    = 16,
            BackColor = C_ContentBg
        };
        sep.Paint += (_, e) =>
            e.Graphics.DrawLine(new Pen(C_FooterLine), 0, 8, 460, 8);
        parent.Controls.Add(sep);
        return y + 16;
    }

    private TextBox PathBox(string text, string placeholder)
    {
        var box = new TextBox
        {
            Text        = string.IsNullOrEmpty(text) ? placeholder : text,
            ReadOnly    = true,
            Font        = new Font("Segoe UI", 9f),
            Height      = 28,
            BackColor   = string.IsNullOrEmpty(text) ? C_White : C_InputBg,
            ForeColor   = string.IsNullOrEmpty(text) ? C_TextGray : C_Blue,
            BorderStyle = BorderStyle.FixedSingle
        };
        return box;
    }

    private static Button ActionButton(string text)
    {
        var btn = new Button
        {
            Text      = text,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 8.5f),
            ForeColor = C_TextDark,
            BackColor = C_White,
            Size      = new Size(62, 28),
            Cursor    = Cursors.Hand
        };
        btn.FlatAppearance.BorderColor = C_Border;
        btn.FlatAppearance.BorderSize  = 1;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
        return btn;
    }

    private static Button FlatButton(string text, Color fg, Color bg)
    {
        var btn = new Button
        {
            Text      = text,
            FlatStyle = FlatStyle.Flat,
            ForeColor = fg,
            BackColor = bg,
            Cursor    = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    // ── Logic ─────────────────────────────────────────────────────────────

    private void RefreshNwfHint()
    {
        if (string.IsNullOrEmpty(_settings.NwfFolder) || !Directory.Exists(_settings.NwfFolder))
        {
            _nwfHint.Text      = "Click Load to scan for .nwf files";
            _nwfHint.ForeColor = C_TextGray;
            return;
        }
        var files = Directory.GetFiles(_settings.NwfFolder, "*.nwf");
        if (files.Length == 0)
        {
            _nwfHint.Text      = "No .nwf files found in this folder";
            _nwfHint.ForeColor = Color.FromArgb(180, 80, 80);
        }
        else
        {
            _nwfHint.Text      = $"{files.Length} file(s): " +
                                  string.Join(", ", files.Select(Path.GetFileName));
            _nwfHint.ForeColor = Color.FromArgb(30, 130, 50);
        }

        _nwfBox.BackColor = C_InputBg;
        _nwfBox.ForeColor = C_Blue;
    }

    private void PollStatus()
    {
        try
        {
            string s = TaskSchedulerService.GetTaskStatus(_settings.TaskName);
            (_statusBadge.Text, _statusBadge.ForeColor) = s switch
            {
                "Ready"    => ($"●  Ready — runs daily at {_settings.ScheduleTime}", Color.FromArgb(30, 140, 60)),
                "Running"  => ("●  Running now…",                                     Color.FromArgb(0, 100, 200)),
                "Disabled" => ("●  Task disabled",                                     Color.FromArgb(180, 120, 0)),
                _          => ("●  No scheduled task found",                           C_TextGray)
            };
        }
        catch
        {
            _statusBadge.Text      = "●  Unable to read status";
            _statusBadge.ForeColor = Color.FromArgb(180, 40, 40);
        }
    }

    private void OnApply(object? sender, EventArgs e)
    {
        try
        {
            var (_, batPath) = ScriptGeneratorService.GenerateScripts(_settings);
            TaskSchedulerService.CreateOrUpdateTask(_settings, batPath);
            PollStatus();
            MessageBox.Show(
                $"Done!\n\nConversion scheduled every day at {_settings.ScheduleTime}.",
                "Schedule Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            LogService.AppendEntry(ex.Message, true);
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnRunNow(object? sender, EventArgs e)
    {
        try
        {
            TaskSchedulerService.RunTaskNow(_settings.TaskName);
            PollStatus();
            MessageBox.Show("Conversion triggered.", "Run Now",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            LogService.AppendEntry(ex.Message, true);
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnRemoveTask(object? sender, EventArgs e)
    {
        if (MessageBox.Show($"Remove scheduled task \"{_settings.TaskName}\"?",
            "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try
        {
            TaskSchedulerService.DeleteTask(_settings.TaskName);
            PollStatus();
        }
        catch (Exception ex)
        {
            LogService.AppendEntry(ex.Message, true);
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
