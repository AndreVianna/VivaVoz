using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace VivaVoz.Migrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class AddLanguageCodeToRecordings : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
        migrationBuilder.AddColumn<string>(
            name: "LanguageCode",
            table: "Recordings",
            type: "TEXT",
            nullable: false,
            defaultValue: "unknown");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
        migrationBuilder.DropColumn(name: "LanguageCode", table: "Recordings");
    }
}
