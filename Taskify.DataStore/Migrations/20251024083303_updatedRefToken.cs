using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taskify.DataStore.Migrations
{
    /// <inheritdoc />
    public partial class updatedRefToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "RefreshTokens",
                newName: "UpdateAT");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateAT",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateAT",
                table: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "UpdateAT",
                table: "RefreshTokens",
                newName: "CreatedAt");
        }
    }
}
