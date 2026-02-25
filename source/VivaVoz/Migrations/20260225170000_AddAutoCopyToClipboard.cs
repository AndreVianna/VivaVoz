using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VivaVoz.Migrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class AddAutoCopyToClipboard : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<bool>(
            name: "AutoCopyToClipboard",
            table: "Settings",
            type: "INTEGER",
            nullable: false,
            defaultValue: true);

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(name: "AutoCopyToClipboard", table: "Settings");
}
