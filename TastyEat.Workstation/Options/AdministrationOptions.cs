namespace TastyEat.Workstation.Options;

public sealed class AdministrationOptions
{
    public int LogArchiveAfterDays { get; set; } = 2;
    public int DatabaseBackupIntervalDays { get; set; } = 3;
    public string ScriptsDirectoryName { get; set; } = "Scripts";
}
