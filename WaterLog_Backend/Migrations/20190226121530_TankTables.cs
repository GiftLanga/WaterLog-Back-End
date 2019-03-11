using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WaterLog_Backend.Migrations
{
    public partial class TankTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TankLevels");

            migrationBuilder.AddColumn<int>(
                name: "FaultCount",
                table: "Segments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FaultCount",
                table: "Monitors",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TankMonitors",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Long = table.Column<double>(nullable: false),
                    Lat = table.Column<double>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    FaultCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TankMonitors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TankReadings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TankMonitorsId = table.Column<int>(nullable: false),
                    PumpId = table.Column<int>(nullable: false),
                    PercentageLevel = table.Column<double>(nullable: false),
                    OptimalLevel = table.Column<double>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TankReadings", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TankMonitors");

            migrationBuilder.DropTable(
                name: "TankReadings");

            migrationBuilder.DropColumn(
                name: "FaultCount",
                table: "Segments");

            migrationBuilder.DropColumn(
                name: "FaultCount",
                table: "Monitors");

            migrationBuilder.CreateTable(
                name: "TankLevels",
                columns: table => new
                {
                    TankId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Instruction = table.Column<string>(nullable: true),
                    LevelStatus = table.Column<string>(nullable: true),
                    Percentage = table.Column<int>(nullable: false),
                    PumpId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TankLevels", x => x.TankId);
                });
        }
    }
}
