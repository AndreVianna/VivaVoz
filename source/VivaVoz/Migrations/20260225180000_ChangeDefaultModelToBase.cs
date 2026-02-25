using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VivaVoz.Migrations;

/// <inheritdoc />
public partial class ChangeDefaultModelToBase : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql(
            "UPDATE \"Settings\" SET \"WhisperModelSize\" = 'base' WHERE \"WhisperModelSize\" = 'tiny'");

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql(
            "UPDATE \"Settings\" SET \"WhisperModelSize\" = 'tiny' WHERE \"WhisperModelSize\" = 'base'");
}
