using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace DaOAuthCore.WebServer.Migrations
{
    public partial class multiple_redirect_url : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefautRedirectUri",
                schema: "auth",
                table: "Clients");

            migrationBuilder.CreateTable(
                name: "ClientReturnUrls",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FK_Client = table.Column<int>(type: "int", nullable: false),
                    ReturnUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientReturnUrls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientReturnUrls_Clients_FK_Client",
                        column: x => x.FK_Client,
                        principalSchema: "auth",
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientReturnUrls_FK_Client",
                schema: "auth",
                table: "ClientReturnUrls",
                column: "FK_Client");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientReturnUrls",
                schema: "auth");

            migrationBuilder.AddColumn<string>(
                name: "DefautRedirectUri",
                schema: "auth",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
