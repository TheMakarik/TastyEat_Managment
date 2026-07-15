using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TastyEat.Workstation.Options;
using TastyEat.Workstation.Services.Interfaces;

namespace TastyEat.Workstation.Services.HostedServices;

public sealed class DatabaseBackupHostedService : BackgroundService
{
    private readonly IApplicationDataService _applicationDataService;
    private readonly IOptions<AdministrationOptions> _options;
    private readonly ILogger<DatabaseBackupHostedService> _logger;

    public DatabaseBackupHostedService(
        IApplicationDataService applicationDataService,
        IOptions<AdministrationOptions> options,
        ILogger<DatabaseBackupHostedService> logger)
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
                if (ShouldCreateBackup())
                    RunDetachedBackup();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start database backup");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private bool ShouldCreateBackup()
    {
        var interval = _options.Value.DatabaseBackupIntervalDays;
        if (interval <= 0)
            return false;

        var backups = Directory.GetFiles(_applicationDataService.BackupsDirectory, "tastyeat_*.db")
            .Select(f => new FileInfo(f))
            .Where(f => f.Exists)
            .OrderByDescending(f => f.LastWriteTime)
            .ToList();

        if (backups.Count == 0)
            return true;

        return backups[0].LastWriteTime < DateTime.Now.AddDays(-interval);
    }

    private void RunDetachedBackup()
    {
        Directory.CreateDirectory(_applicationDataService.BackupsDirectory);
        Directory.CreateDirectory(_applicationDataService.ScriptsDirectory);

        var fileName = $"tastyeat_{DateTime.Now:yyyyMMdd_HHmmss}.db";
        var targetPath = Path.Join(_applicationDataService.BackupsDirectory, fileName);

        if (OperatingSystem.IsWindows())
        {
            var scriptPath = Path.Join(_applicationDataService.ScriptsDirectory, "backup.bat");
            File.WriteAllText(scriptPath,
                $"@echo off{Environment.NewLine}" +
                $"copy /Y \"{_applicationDataService.DatabasePath}\" \"{targetPath}\"{Environment.NewLine}");

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{scriptPath}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }
        else
        {
            var scriptPath = Path.Join(_applicationDataService.ScriptsDirectory, "backup.sh");
            File.WriteAllText(scriptPath,
                $"#!/bin/bash{Environment.NewLine}" +
                $"cp \"{_applicationDataService.DatabasePath}\" \"{targetPath}\"{Environment.NewLine}");

            Process.Start(new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-c \"nohup bash \\\"{scriptPath}\\\" > /dev/null 2>&1 &\"",
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }

        _logger.LogInformation("Started detached database backup to {TargetPath}", targetPath);
    }
}
