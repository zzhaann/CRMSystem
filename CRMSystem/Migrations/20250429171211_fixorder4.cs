﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMSystem.Migrations
{
    /// <inheritdoc />
    public partial class fixorder4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Florists_FloristId",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "FloristId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Florists_FloristId",
                table: "Orders",
                column: "FloristId",
                principalTable: "Florists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Florists_FloristId",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "FloristId",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Florists_FloristId",
                table: "Orders",
                column: "FloristId",
                principalTable: "Florists",
                principalColumn: "Id");
        }
    }
}
