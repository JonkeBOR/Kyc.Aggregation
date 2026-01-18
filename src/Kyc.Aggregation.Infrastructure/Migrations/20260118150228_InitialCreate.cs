using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kyc.Aggregation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerKycSnapshots",
                columns: table => new
                {
                    Ssn = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DataJson = table.Column<string>(type: "TEXT", nullable: false),
                    FetchedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerKycSnapshots", x => x.Ssn);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerKycSnapshots_FetchedAtUtc",
                table: "CustomerKycSnapshots",
                column: "FetchedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerKycSnapshots");
        }
    }
}
