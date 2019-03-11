using Microsoft.EntityFrameworkCore.Migrations;

namespace WaterLog_Backend.Migrations
{
    public partial class update_tankMonitors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "connectedMonitorID",
                table: "TankMonitors",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "connectedMonitorType",
                table: "TankMonitors",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "connectedMonitorID",
                table: "TankMonitors");

            migrationBuilder.DropColumn(
                name: "connectedMonitorType",
                table: "TankMonitors");
        }
    }
}
