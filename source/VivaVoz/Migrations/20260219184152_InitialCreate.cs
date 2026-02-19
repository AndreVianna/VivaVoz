using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VivaVoz.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
        migrationBuilder.CreateTable(
            name: "Recordings",
            columns: table => new {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                AudioFileName = table.Column<string>(type: "TEXT", nullable: false),
                Transcript = table.Column<string>(type: "TEXT", nullable: true),
                Status = table.Column<string>(type: "TEXT", nullable: false),
                Language = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "auto"),
                Duration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                WhisperModel = table.Column<string>(type: "TEXT", nullable: false),
                FileSize = table.Column<long>(type: "INTEGER", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Recordings", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Settings",
            columns: table => new {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                HotkeyConfig = table.Column<string>(type: "TEXT", nullable: false),
                WhisperModelSize = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "tiny"),
                AudioInputDevice = table.Column<string>(type: "TEXT", nullable: true),
                StoragePath = table.Column<string>(type: "TEXT", nullable: false),
                ExportFormat = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "MP3"),
                Theme = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "System"),
                Language = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "auto"),
                AutoUpdate = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
            },
            constraints: table => table.PrimaryKey("PK_Settings", x => x.Id));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
        migrationBuilder.DropTable(
            name: "Recordings");

        migrationBuilder.DropTable(
            name: "Settings");
    }
}
