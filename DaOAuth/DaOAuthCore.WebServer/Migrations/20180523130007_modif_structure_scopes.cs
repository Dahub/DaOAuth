using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace DaOAuthCore.WebServer.Migrations
{
    public partial class modif_structure_scopes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scopes_Clients_FK_Client",
                schema: "auth",
                table: "Scopes");

            migrationBuilder.DropIndex(
                name: "IX_Scopes_FK_Client",
                schema: "auth",
                table: "Scopes");

            migrationBuilder.DropColumn(
                name: "FK_Client",
                schema: "auth",
                table: "Scopes");

            migrationBuilder.CreateTable(
                name: "ClientsScopes",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FK_Client = table.Column<int>(type: "int", nullable: false),
                    FK_Scope = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientsScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientsScopes_Clients_FK_Client",
                        column: x => x.FK_Client,
                        principalSchema: "auth",
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientsScopes_Scopes_FK_Scope",
                        column: x => x.FK_Scope,
                        principalSchema: "auth",
                        principalTable: "Scopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientsScopes_FK_Client",
                schema: "auth",
                table: "ClientsScopes",
                column: "FK_Client");

            migrationBuilder.CreateIndex(
                name: "IX_ClientsScopes_FK_Scope",
                schema: "auth",
                table: "ClientsScopes",
                column: "FK_Scope");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientsScopes",
                schema: "auth");

            migrationBuilder.AddColumn<int>(
                name: "FK_Client",
                schema: "auth",
                table: "Scopes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Scopes_FK_Client",
                schema: "auth",
                table: "Scopes",
                column: "FK_Client");

            migrationBuilder.AddForeignKey(
                name: "FK_Scopes_Clients_FK_Client",
                schema: "auth",
                table: "Scopes",
                column: "FK_Client",
                principalSchema: "auth",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
