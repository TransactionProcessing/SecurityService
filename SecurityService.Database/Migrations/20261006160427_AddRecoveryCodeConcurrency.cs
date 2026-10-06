using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityService.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddRecoveryCodeConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "RecoveryCodes",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValueSql: "CONVERT(nvarchar(32), NEWID())");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "RecoveryCodes");
        }
    }
}
