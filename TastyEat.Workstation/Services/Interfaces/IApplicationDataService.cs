namespace TastyEat.Workstation.Services.Interfaces;

public interface IApplicationDataService
{
    string ApplicationDirectory { get; }
    string LogsDirectory { get; }
    string BackupsDirectory { get; }
    string ScriptsDirectory { get; }
    string DatabasePath { get; }
    Task EnsureDirectoriesAndDatabaseExistAsync(CancellationToken cancellationToken = default);
    void OpenLogsFolder();
}
