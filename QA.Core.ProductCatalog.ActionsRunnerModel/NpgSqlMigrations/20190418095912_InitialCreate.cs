using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace QA.Core.ProductCatalog.ActionsRunnerModel.NpgSqlMigrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "schedules",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    enabled = table.Column<bool>(nullable: false),
                    cron_expression = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_schedules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "task_states",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_states", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tasks",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created_time = table.Column<DateTime>(nullable: false),
                    last_status_change_time = table.Column<DateTime>(nullable: true),
                    name = table.Column<string>(nullable: true),
                    state_id = table.Column<int>(nullable: false),
                    data = table.Column<string>(nullable: true),
                    user_id = table.Column<int>(nullable: false),
                    user_name = table.Column<string>(nullable: true),
                    progress = table.Column<byte>(nullable: true),
                    message = table.Column<string>(nullable: true),
                    is_cancellation_requested = table.Column<bool>(nullable: false),
                    is_cancelled = table.Column<bool>(nullable: false),
                    display_name = table.Column<string>(nullable: true),
                    scheduled_from_task_id = table.Column<int>(nullable: true),
                    schedule_id = table.Column<int>(nullable: true),
                    exclusive_category = table.Column<string>(nullable: true),
                    config = table.Column<string>(nullable: true),
                    bin_data = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_tasks_schedules_schedule_id",
                        column: x => x.schedule_id,
                        principalTable: "schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tasks_tasks_scheduled_from_task_id",
                        column: x => x.scheduled_from_task_id,
                        principalTable: "tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tasks_task_states_state_id",
                        column: x => x.state_id,
                        principalTable: "task_states",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "task_states",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "New" },
                    { 2, "Running" },
                    { 3, "Completed" },
                    { 4, "Error" },
                    { 5, "Cancelled" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_tasks_schedule_id",
                table: "tasks",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_scheduled_from_task_id",
                table: "tasks",
                column: "scheduled_from_task_id");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_state_id",
                table: "tasks",
                column: "state_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "tasks");
            migrationBuilder.DropTable(name: "schedules");
            migrationBuilder.DropTable(name: "task_states");
        }
    }
}
