namespace NWDUpdater.UI;

public static class ControlFactory
{
    public static Button CreateAccentButton(string text, EventHandler onClick)
    {
        var btn = new Button
        {
            Text = text,
            Tag = "accent",
            FlatStyle = FlatStyle.Flat,
            BackColor = ThemeManager.Accent,
            ForeColor = Color.Black,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Padding = new Padding(12, 6, 12, 6),
            Cursor = Cursors.Hand,
            Height = 36,
            AutoSize = true
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += onClick;
        return btn;
    }

    public static Button CreateNavButton(string text, string icon)
    {
        var btn = new Button
        {
            Text = $"  {icon}   {text}",
            Tag = "nav",
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 10f),
            Height = 44,
            Dock = DockStyle.Top,
            Padding = new Padding(16, 0, 0, 0),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    public static Button CreateButton(string text, EventHandler onClick, string? tag = null)
    {
        var btn = new Button
        {
            Text = text,
            Tag = tag,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f),
            Padding = new Padding(10, 4, 10, 4),
            Cursor = Cursors.Hand,
            Height = 32,
            AutoSize = true
        };
        btn.FlatAppearance.BorderSize = 1;
        btn.FlatAppearance.BorderColor = ThemeManager.Border;
        btn.Click += onClick;
        return btn;
    }

    public static Button CreateDangerButton(string text, EventHandler onClick)
    {
        var btn = new Button
        {
            Text = text,
            Tag = "danger",
            FlatStyle = FlatStyle.Flat,
            BackColor = ThemeManager.Error,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Padding = new Padding(10, 4, 10, 4),
            Cursor = Cursors.Hand,
            Height = 32,
            AutoSize = true
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += onClick;
        return btn;
    }

    public static Panel CreateCard()
    {
        var panel = new Panel
        {
            Tag = "card",
            BackColor = ThemeManager.SurfaceElevated,
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 0, 8),
            Height = 80
        };
        return panel;
    }

    public static Label CreateLabel(string text, bool secondary = false, float fontSize = 9f, bool bold = false)
    {
        return new Label
        {
            Text = text,
            Tag = secondary ? "secondary" : null,
            ForeColor = secondary ? ThemeManager.TextSecondary : ThemeManager.TextPrimary,
            Font = new Font("Segoe UI", fontSize, bold ? FontStyle.Bold : FontStyle.Regular),
            AutoSize = true,
            BackColor = Color.Transparent
        };
    }

    public static Label CreateStatusBadge(string text, Color dotColor)
    {
        return new Label
        {
            Text = $"●  {text}",
            ForeColor = dotColor,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            AutoSize = true,
            BackColor = Color.Transparent
        };
    }
}
