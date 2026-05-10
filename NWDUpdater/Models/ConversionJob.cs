namespace NWDUpdater.Models;

public class ConversionJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string NwfPath { get; set; } = string.Empty;
    public string NwdPath { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}
