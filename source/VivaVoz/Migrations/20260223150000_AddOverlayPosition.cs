using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VivaVoz.Migrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class AddOverlayPosition : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
        migrationBuilder.AddColumn<int>(
            name: "OverlayX",
            table: "Settings",
            type: "INTEGER",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "OverlayY",
            table: "Settings",
            type: "INTEGER",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
        migrationBuilder.DropColumn(name: "OverlayX", table: "Settings");
        migrationBuilder.DropColumn(name: "OverlayY", table: "Settings");
    }
}
