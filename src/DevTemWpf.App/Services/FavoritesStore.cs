using System.IO;
using Microsoft.Data.Sqlite;

namespace DevTemWpf.App.Services;

/// <summary>
/// Tiny SQLite store (activity favorites). Per-operation connections, so no
/// lock is ever held across calls and backup can copy the file. Schema
/// versioned with <c>PRAGMA user_version</c>.
/// </summary>
public sealed class FavoritesStore
{
    private const int SchemaVersion = 1;

    private readonly string _dbPath;

    public FavoritesStore(string? dbPath = null)
    {
        _dbPath = dbPath ?? AppPaths.DbPath;
    }

    public void EnsureCreated()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            PRAGMA journal_mode=WAL;
            CREATE TABLE IF NOT EXISTS favorites (
                activity_id INTEGER PRIMARY KEY
            );
            """;
        command.ExecuteNonQuery();
        SetVersion(connection, SchemaVersion);
    }

    public bool IsFavorite(int activityId)
    {
        EnsureCreated();
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM favorites WHERE activity_id = $id;";
        command.Parameters.AddWithValue("$id", activityId);
        return (long)command.ExecuteScalar()! > 0;
    }

    public bool ToggleFavorite(int activityId)
    {
        EnsureCreated();
        var was = IsFavorite(activityId);
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = was
            ? "DELETE FROM favorites WHERE activity_id = $id;"
            : "INSERT OR IGNORE INTO favorites (activity_id) VALUES ($id);";
        command.Parameters.AddWithValue("$id", activityId);
        command.ExecuteNonQuery();
        return !was;
    }

    public HashSet<int> GetFavorites()
    {
        EnsureCreated();
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT activity_id FROM favorites;";
        using var reader = command.ExecuteReader();
        var ids = new HashSet<int>();
        while (reader.Read())
        {
            ids.Add(reader.GetInt32(0));
        }

        return ids;
    }

    private SqliteConnection Open()
    {
        try
        {
            var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            return connection;
        }
        catch (SqliteException)
        {
            // Corrupt database: start over rather than fail the app.
            File.Delete(_dbPath);
            var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            return connection;
        }
    }

    private static void SetVersion(SqliteConnection connection, int version)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA user_version={version};";
        command.ExecuteNonQuery();
    }
}
