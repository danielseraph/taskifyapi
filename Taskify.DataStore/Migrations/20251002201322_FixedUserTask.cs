using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taskify.DataStore.Migrations
{
    /// <inheritdoc />
    public partial class FixedUserTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_AspNetUsers_CreatedByUserId1",
                table: "TaskItems");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProjects_AspNetUsers_UserId1",
                table: "UserProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTasks_AspNetUsers_UserId1",
                table: "UserTasks");

            migrationBuilder.DropIndex(
                name: "IX_UserTasks_UserId1",
                table: "UserTasks");

            migrationBuilder.DropIndex(
                name: "IX_UserProjects_UserId1",
                table: "UserProjects");

            migrationBuilder.DropIndex(
                name: "IX_TaskItems_CreatedByUserId1",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserProjects");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId1",
                table: "TaskItems");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserTasks",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserProjects",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "TaskItems",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_CreatedByUserId",
                table: "TaskItems",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_AspNetUsers_CreatedByUserId",
                table: "TaskItems",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProjects_AspNetUsers_UserId",
                table: "UserProjects",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTasks_AspNetUsers_UserId",
                table: "UserTasks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_AspNetUsers_CreatedByUserId",
                table: "TaskItems");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProjects_AspNetUsers_UserId",
                table: "UserProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTasks_AspNetUsers_UserId",
                table: "UserTasks");

            migrationBuilder.DropIndex(
                name: "IX_TaskItems_CreatedByUserId",
                table: "TaskItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "UserTasks",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "UserTasks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "UserProjects",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "UserProjects",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedByUserId",
                table: "TaskItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId1",
                table: "TaskItems",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTasks_UserId1",
                table: "UserTasks",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjects_UserId1",
                table: "UserProjects",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_CreatedByUserId1",
                table: "TaskItems",
                column: "CreatedByUserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_AspNetUsers_CreatedByUserId1",
                table: "TaskItems",
                column: "CreatedByUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProjects_AspNetUsers_UserId1",
                table: "UserProjects",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTasks_AspNetUsers_UserId1",
                table: "UserTasks",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
