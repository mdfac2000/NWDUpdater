namespace NWDUpdater.UI;

public enum AppTheme { Dark, Light }

public static class ThemeManager
{
    public static AppTheme CurrentTheme { get; private set; } = AppTheme.Dark;

    public static Color Background { get; private set; }
    public static Color Surface { get; private set; }
    public static Color SurfaceElevated { get; private set; }
    public static Color Border { get; private set; }
    public static Color TextPrimary { get; private set; }
    public static Color TextSecondary { get; private set; }
    public static Color Accent { get; private set; }
    public static Color AccentHover { get; private set; }
    public static Color Success { get; private set; }
    public static Color Error { get; private set; }
    public static Color Warning { get; private set; }

    static ThemeManager()
    {
        SetTheme(AppTheme.Dark);
    }

    public static void SetTheme(AppTheme theme)
    {
        CurrentTheme = theme;

        if (theme == AppTheme.Dark)
        {
            Background = ColorTranslator.FromHtml("#1A1A1A");
            Surface = ColorTranslator.FromHtml("#252525");
            SurfaceElevated = ColorTranslator.FromHtml("#2E2E2E");
            Border = ColorTranslator.FromHtml("#3A3A3A");
            TextPrimary = ColorTranslator.FromHtml("#F0F0F0");
            TextSecondary = ColorTranslator.FromHtml("#A0A0A0");
            Accent = ColorTranslator.FromHtml("#F5C400");
            AccentHover = ColorTranslator.FromHtml("#FFD740");
            Success = ColorTranslator.FromHtml("#4CAF50");
            Error = ColorTranslator.FromHtml("#F44336");
            Warning = ColorTranslator.FromHtml("#FF9800");
        }
        else
        {
            Background = ColorTranslator.FromHtml("#F5F5F5");
            Surface = ColorTranslator.FromHtml("#FFFFFF");
            SurfaceElevated = ColorTranslator.FromHtml("#FAFAFA");
            Border = ColorTranslator.FromHtml("#E0E0E0");
            TextPrimary = ColorTranslator.FromHtml("#1A1A1A");
            TextSecondary = ColorTranslator.FromHtml("#666666");
            Accent = ColorTranslator.FromHtml("#E6A800");
            AccentHover = ColorTranslator.FromHtml("#FFD740");
            Success = ColorTranslator.FromHtml("#4CAF50");
            Error = ColorTranslator.FromHtml("#F44336");
            Warning = ColorTranslator.FromHtml("#FF9800");
        }
    }

    public static void Apply(Control root)
    {
        ApplyToControl(root);
        foreach (Control child in root.Controls)
        {
            Apply(child);
        }
    }

    private static void ApplyToControl(Control control)
    {
        switch (control)
        {
            case Form form:
                form.BackColor = Background;
                form.ForeColor = TextPrimary;
                break;

            case Button btn:
                if (btn.Tag is "accent")
                {
                    btn.BackColor = Accent;
                    btn.ForeColor = Color.Black;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.FlatAppearance.MouseOverBackColor = AccentHover;
                }
                else if (btn.Tag is "danger")
                {
                    btn.BackColor = Error;
                    btn.ForeColor = Color.White;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                }
                else if (btn.Tag is "nav")
                {
                    btn.BackColor = Surface;
                    btn.ForeColor = TextPrimary;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.FlatAppearance.MouseOverBackColor = SurfaceElevated;
                }
                else
                {
                    btn.BackColor = SurfaceElevated;
                    btn.ForeColor = TextPrimary;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = Border;
                    btn.FlatAppearance.MouseOverBackColor = Border;
                }
                break;

            case TextBox tb:
                tb.BackColor = SurfaceElevated;
                tb.ForeColor = TextPrimary;
                tb.BorderStyle = BorderStyle.FixedSingle;
                break;

            case RichTextBox rtb:
                rtb.BackColor = SurfaceElevated;
                rtb.ForeColor = TextPrimary;
                break;

            case Label lbl:
                if (lbl.Tag is "secondary")
                    lbl.ForeColor = TextSecondary;
                else if (lbl.Tag is "accent")
                    lbl.ForeColor = Accent;
                else
                    lbl.ForeColor = TextPrimary;
                lbl.BackColor = Color.Transparent;
                break;

            case FlowLayoutPanel flp:
                flp.BackColor = Background;
                break;

            case Panel panel:
                if (panel.Tag is "sidebar")
                    panel.BackColor = Surface;
                else if (panel.Tag is "card")
                    panel.BackColor = SurfaceElevated;
                else if (panel.Tag is "statusbar")
                    panel.BackColor = Surface;
                else
                    panel.BackColor = Background;
                break;

            case CheckBox cb:
                cb.ForeColor = TextPrimary;
                cb.BackColor = Color.Transparent;
                break;

            case RadioButton rb:
                rb.ForeColor = TextPrimary;
                rb.BackColor = Color.Transparent;
                break;

            case DateTimePicker dtp:
                dtp.CalendarMonthBackground = SurfaceElevated;
                dtp.CalendarForeColor = TextPrimary;
                break;

            case GroupBox gb:
                gb.ForeColor = TextPrimary;
                gb.BackColor = Background;
                break;

            case StatusStrip ss:
                ss.BackColor = Surface;
                ss.ForeColor = TextPrimary;
                foreach (ToolStripItem item in ss.Items)
                {
                    item.ForeColor = TextPrimary;
                    item.BackColor = Surface;
                }
                break;

            case UserControl uc:
                uc.BackColor = Background;
                uc.ForeColor = TextPrimary;
                break;

            default:
                control.BackColor = Background;
                control.ForeColor = TextPrimary;
                break;
        }
    }
}
