using System.IO;
using System.IO.Compression;

namespace DevTemWpf.App.Services;

/// <summary>
/// Backup/restore of user state (settings + database). Explicit paths,
/// validation on restore, restart recommended afterwards.
/// </summary>
public interface IBackupService
{
    string CreateBackup(string? destinationDir = null);

    void RestoreBackup(string zipPath);
}
