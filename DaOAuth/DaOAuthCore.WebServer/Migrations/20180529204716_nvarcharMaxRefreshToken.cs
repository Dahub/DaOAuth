using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace DaOAuthCore.WebServer.Migrations
{
    public partial class nvarcharMaxRefreshToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                schema: "auth",
                table: "UsersClients",
                type: "nvarchar(max)",
                maxLength: 2147483647,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                schema: "auth",
                table: "UsersClients",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 2147483647,
                oldNullable: true);
        }
    }
}
