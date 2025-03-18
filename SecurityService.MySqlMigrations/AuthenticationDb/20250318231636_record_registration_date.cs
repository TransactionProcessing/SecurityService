using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityService.MySqlMigrations.AuthenticationDb
{
    /// <inheritdoc />
    public partial class record_registration_date : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationDateTime",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationDateTime",
                table: "AspNetUsers");
        }
    }
}
