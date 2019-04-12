using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace QA.Core.DPC.Front.Migrations.NpgSqlMigrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "product_versions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    deleted = table.Column<bool>(nullable: false),
                    modification = table.Column<DateTime>(nullable: false),
                    dpc_id = table.Column<int>(nullable: false),
                    slug = table.Column<string>(nullable: true),
                    version = table.Column<int>(nullable: false),
                    is_live = table.Column<bool>(nullable: false),
                    language = table.Column<string>(nullable: true),
                    format = table.Column<string>(nullable: true),
                    data = table.Column<string>(nullable: true),
                    alias = table.Column<string>(nullable: true),
                    created = table.Column<DateTime>(nullable: false),
                    updated = table.Column<DateTime>(nullable: false),
                    hash = table.Column<string>(nullable: true),
                    marketing_product_id = table.Column<int>(nullable: true),
                    title = table.Column<string>(nullable: true),
                    user_updated = table.Column<string>(nullable: true),
                    user_updated_id = table.Column<int>(nullable: true),
                    product_type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_versions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    dpc_id = table.Column<int>(nullable: false),
                    slug = table.Column<string>(nullable: true),
                    version = table.Column<int>(nullable: false),
                    is_live = table.Column<bool>(nullable: false),
                    language = table.Column<string>(nullable: true),
                    format = table.Column<string>(nullable: true),
                    data = table.Column<string>(nullable: true),
                    alias = table.Column<string>(nullable: true),
                    created = table.Column<DateTime>(nullable: false),
                    updated = table.Column<DateTime>(nullable: false),
                    hash = table.Column<string>(nullable: true),
                    marketing_product_id = table.Column<int>(nullable: true),
                    title = table.Column<string>(nullable: true),
                    user_updated = table.Column<string>(nullable: true),
                    user_updated_id = table.Column<int>(nullable: true),
                    product_type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "region_updates",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    updated = table.Column<DateTime>(nullable: false),
                    region_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_region_updates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_region_versions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    product_version_id = table.Column<int>(nullable: false),
                    region_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_region_versions", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_region_versions_product_versions_product_version_id",
                        column: x => x.product_version_id,
                        principalSchema: "public",
                        principalTable: "product_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_regions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    product_id = table.Column<int>(nullable: false),
                    region_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_regions", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_regions_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "public",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_region_versions_product_version_id",
                schema: "public",
                table: "product_region_versions",
                column: "product_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_regions_product_id",
                schema: "public",
                table: "product_regions",
                column: "product_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_region_versions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "product_regions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "region_updates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "product_versions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "products",
                schema: "public");
        }
    }
}
