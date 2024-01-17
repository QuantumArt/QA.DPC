using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace QA.Core.DPC.DAL.NpgSqlMigrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    channel = table.Column<string>(nullable: true),
                    created = table.Column<DateTime>(nullable: false),
                    data = table.Column<string>(nullable: true),
                    method = table.Column<string>(nullable: true),
                    user_id = table.Column<int>(nullable: false),
                    user_name = table.Column<string>(nullable: true),
                    data_key = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_messages", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "messages");
        }
    }
}
