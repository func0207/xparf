using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xparf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FinalBackendModelSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 19L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "PlatformSettings",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "TopupPackages",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 19L,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 30, 4, 37, 18, 702, DateTimeKind.Utc).AddTicks(8633));

            migrationBuilder.UpdateData(
                table: "PlatformSettings",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 30, 4, 37, 18, 703, DateTimeKind.Utc).AddTicks(295));

            migrationBuilder.UpdateData(
                table: "TopupPackages",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 30, 4, 37, 18, 703, DateTimeKind.Utc).AddTicks(2161));
        }
    }
}
