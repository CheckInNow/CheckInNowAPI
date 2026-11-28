using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddSessionCheckIn : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SessionCheckIns",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                RegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                CheckedInAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SessionCheckIns", x => x.Id);
                table.ForeignKey(
                    name: "FK_SessionCheckIns_Registrations_RegistrationId",
                    column: x => x.RegistrationId,
                    principalTable: "Registrations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_SessionCheckIns_Sessions_SessionId",
                    column: x => x.SessionId,
                    principalTable: "Sessions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SessionCheckIns_RegistrationId_SessionId",
            table: "SessionCheckIns",
            columns: new[] { "RegistrationId", "SessionId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_SessionCheckIns_SessionId",
            table: "SessionCheckIns",
            column: "SessionId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SessionCheckIns");
    }
}
