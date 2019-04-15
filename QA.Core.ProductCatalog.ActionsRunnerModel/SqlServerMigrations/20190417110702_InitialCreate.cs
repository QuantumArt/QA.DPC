using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace QA.Core.ProductCatalog.ActionsRunnerModel.SqlServerMigrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Enabled = table.Column<bool>(nullable: false),
                    CronExpression = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TaskStates",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskStates", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    LastStatusChangeTime = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    StateID = table.Column<int>(nullable: false),
                    Data = table.Column<string>(nullable: true),
                    UserID = table.Column<int>(nullable: false),
                    UserName = table.Column<string>(nullable: true),
                    Progress = table.Column<byte>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    IsCancellationRequested = table.Column<bool>(nullable: false),
                    IsCancelled = table.Column<bool>(nullable: false),
                    DisplayName = table.Column<string>(nullable: true),
                    ScheduledFromTaskID = table.Column<int>(nullable: true),
                    ScheduleID = table.Column<int>(nullable: true),
                    ExclusiveCategory = table.Column<string>(nullable: true),
                    Config = table.Column<string>(nullable: true),
                    BinData = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tasks_Schedules_ScheduleID",
                        column: x => x.ScheduleID,
                        principalTable: "Schedules",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Tasks_ScheduledFromTaskID",
                        column: x => x.ScheduledFromTaskID,
                        principalTable: "Tasks",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_TaskStates_StateID",
                        column: x => x.StateID,
                        principalTable: "TaskStates",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "TaskStates",
                columns: new[] { "ID", "Name" },
                values: new object[,]
                {
                    { 1, "New" },
                    { 2, "Running" },
                    { 3, "Completed" },
                    { 4, "Error" },
                    { 5, "Cancelled" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ScheduleID",
                table: "Tasks",
                column: "ScheduleID");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ScheduledFromTaskID",
                table: "Tasks",
                column: "ScheduledFromTaskID");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_StateID",
                table: "Tasks",
                column: "StateID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "TaskStates");
        }
    }
}
