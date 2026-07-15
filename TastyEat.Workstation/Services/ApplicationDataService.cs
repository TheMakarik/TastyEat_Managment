using System.Diagnostics;
using Microsoft.Extensions.Options;
using TastyEat.Workstation.Options;
using TastyEat.Workstation.Services.Interfaces;

namespace TastyEat.Workstation.Services;

public sealed class ApplicationDataService : IApplicationDataService
{
    public string ApplicationDirectory { get; }
    public string LogsDirectory { get; }
    public string BackupsDirectory { get; }
    public string ScriptsDirectory { get; }
    public string DatabasePath { get; }

    public ApplicationDataService(IOptions<AdministrationOptions> options)
    {
        var administrationOptions = options.Value;

        ApplicationDirectory = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "tasty-eat");

        LogsDirectory = Path.Join(ApplicationDirectory, "logs");
        BackupsDirectory = Path.Join(ApplicationDirectory, "backups");
        ScriptsDirectory = Path.Join(ApplicationDirectory, administrationOptions.ScriptsDirectoryName);
        DatabasePath = Path.Join(ApplicationDirectory, "tastyeat.db");
    }

    public Task EnsureDirectoriesAndDatabaseExistAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(ApplicationDirectory);
        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(BackupsDirectory);
        Directory.CreateDirectory(ScriptsDirectory);

        if (!File.Exists(DatabasePath))
            File.WriteAllBytes(DatabasePath, []);

        return Task.CompletedTask;
    }

    public void OpenLogsFolder()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = LogsDirectory,
            UseShellExecute = true
        });
    }
}
