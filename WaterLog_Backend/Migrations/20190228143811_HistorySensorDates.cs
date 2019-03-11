using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WaterLog_Backend.Migrations
{
    public partial class HistorySensorDates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "HistoryLogs",
                newName: "ManualDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "AutomaticDate",
                table: "HistoryLogs",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "HistoryLogs",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutomaticDate",
                table: "HistoryLogs");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "HistoryLogs");

            migrationBuilder.RenameColumn(
                name: "ManualDate",
                table: "HistoryLogs",
                newName: "Date");
        }
    }
}
