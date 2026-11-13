using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddSessionOverlapConstraint : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

        // Two active sessions in the same room cannot overlap ([) ranges: back-to-back sessions are allowed).
        migrationBuilder.Sql("""
            ALTER TABLE "Sessions" ADD CONSTRAINT "EX_Sessions_Room_NoOverlap"
            EXCLUDE USING gist ("RoomId" WITH =, tstzrange("StartTime", "EndTime") WITH &&)
            WHERE ("DeletedAt" IS NULL);
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""ALTER TABLE "Sessions" DROP CONSTRAINT "EX_Sessions_Room_NoOverlap";""");
    }
}
