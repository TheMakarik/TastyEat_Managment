namespace TastyEat.Workstation.Services.Interfaces;

public interface IBackupService
{
    Task<string> CreateBackupAsync(string targetDirectory, CancellationToken cancellationToken = default);
    Task RestoreBackupAsync(string backupFilePath, CancellationToken cancellationToken = default);
    Task<string> CreateScheduledBackupAsync(CancellationToken cancellationToken = default);
    void OpenLogsFolder();
}
