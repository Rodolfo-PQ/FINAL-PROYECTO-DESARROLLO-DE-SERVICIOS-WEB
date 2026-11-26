namespace AutoFixManager.Api.Interfaces;

public interface ISqliteBackupService
{
    Task<SqliteBackupResult> CreateBackupAsync(CancellationToken cancellationToken);
}

public sealed record SqliteBackupResult(
    string FileName,
    DateTimeOffset CreatedAt,
    IReadOnlyDictionary<string, int> RecordCounts);
