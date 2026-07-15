using System.Diagnostics;
using System.IO.Compression;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TastyEat.Workstation.Options;
using TastyEat.Workstation.Services.Interfaces;

namespace TastyEat.Workstation.Services.HostedServices;

public sealed class LogArchiveHostedService : BackgroundService
{
    private readonly IApplicationDataService _applicationDataService;
    private readonly IOptions<AdministrationOptions> _options;
    private readonly ILogger<LogArchiveHostedService> _logger;

    public LogArchiveHostedService(
        IApplicationDataService applicationDataService,
        IOptions<AdministrationOptions> options,
        ILogger<LogArchiveHostedService> logger)
    {
        _applicationDataService = applicationDataService;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                ArchiveOldLogs();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to archive old log files");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private void ArchiveOldLogs()
    {
        var threshold = DateTime.Now.AddDays(-_options.Value.LogArchiveAfterDays);
        var logFiles = Directory.GetFiles(_applicationDataService.LogsDirectory, "log-*.txt")
            .Where(f => File.GetLastWriteTime(f) < threshold)
            .ToList();

        if (logFiles.Count == 0)
            return;

        var archiveName = $"logs_{DateTime.Now:yyyyMMdd_HHmmss}.7z";
        var archivePath = Path.Join(_applicationDataService.LogsDirectory, archiveName);

        using var stream = File.Create(archivePath);
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            foreach (var file in logFiles)
            {
                archive.CreateEntryFromFile(file, Path.GetFileName(file));
            }
        }

        foreach (var file in logFiles)
        {
            File.Delete(file);
        }

        _logger.LogInformation(
            "Archived {Count} old log files to {ArchivePath}",
            logFiles.Count,
            archivePath);
    }
}
