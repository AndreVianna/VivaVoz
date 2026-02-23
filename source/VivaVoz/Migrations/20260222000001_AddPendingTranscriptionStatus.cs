using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

using System.Diagnostics.CodeAnalysis;

namespace VivaVoz.Migrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class AddPendingTranscriptionStatus : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
        // Convert any Transcribing records to PendingTranscription.
        // These were left in-flight if the app crashed during transcription.
        // Startup recovery also handles this at runtime, but this migration
        // ensures historical data is consistent on first upgrade.
        migrationBuilder.Sql(
            "UPDATE Recordings SET Status = 'PendingTranscription' WHERE Status = 'Transcribing'");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
        migrationBuilder.Sql(
            "UPDATE Recordings SET Status = 'Transcribing' WHERE Status = 'PendingTranscription'");
    }
}
