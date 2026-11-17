using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddRegistrationTicketCode : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "TicketCode",
            table: "Registrations",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.Sql("UPDATE \"Registrations\" SET \"TicketCode\" = gen_random_uuid();");

        migrationBuilder.CreateIndex(
            name: "IX_Registrations_TicketCode",
            table: "Registrations",
            column: "TicketCode",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Registrations_TicketCode",
            table: "Registrations");

        migrationBuilder.DropColumn(
            name: "TicketCode",
            table: "Registrations");
    }
}
