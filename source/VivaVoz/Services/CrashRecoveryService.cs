using System.Text.Json;

namespace VivaVoz.Services;

/// <summary>
/// File-system-backed crash recovery service.
/// AudioRecorderService writes a JSON marker when recording starts and removes it on clean stop.
/// This service reads that marker on startup to detect any orphaned recordings.
/// </summary>
public class CrashRecoveryService(string? markerPath = null) : ICrashRecoveryService {
    private readonly string _markerPath = markerPath ?? FilePaths.RecoveryMarkerFile;

    /// <inheritdoc />
    public bool HasOrphan() {
        var path = GetOrphanPath();
        if (path is null) {
            // Marker exists but is corrupt or empty — auto-dismiss to avoid accumulation.
            if (File.Exists(_markerPath)) {
                Log.Warning("[CrashRecoveryService] Dismissing unreadable recovery marker at {Path}.", _markerPath);
                Dismiss();
            }

            return false;
        }

        return File.Exists(path);
    }

    private static readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    /// <inheritdoc />
    public string? GetOrphanPath() {
        if (!File.Exists(_markerPath))
            return null;

        try {
            var json = File.ReadAllText(_markerPath);
            var marker = JsonSerializer.Deserialize<RecoveryMarker>(json,
                _options);
            return marker?.FilePath;
        }
        catch (Exception ex) {
            Log.Warning(ex, "[CrashRecoveryService] Failed to parse recovery marker at {Path}.", _markerPath);
            return null;
        }
    }

    /// <inheritdoc />
    public void Dismiss() {
        try {
            if (File.Exists(_markerPath))
                File.Delete(_markerPath);
        }
        catch (Exception ex) {
            Log.Warning(ex, "[CrashRecoveryService] Failed to delete recovery marker at {Path}.", _markerPath);
        }
    }

    private sealed record RecoveryMarker(string FilePath, DateTime StartedAt);
}
