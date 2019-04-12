using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace QA.Core.DPC.Front.Migrations.SqlServerMigrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DpcId = table.Column<int>(nullable: false),
                    Slug = table.Column<string>(nullable: true),
                    Version = table.Column<int>(nullable: false),
                    IsLive = table.Column<bool>(nullable: false),
                    Language = table.Column<string>(nullable: true),
                    Format = table.Column<string>(nullable: true),
                    Data = table.Column<string>(nullable: true),
                    Alias = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: false),
                    Hash = table.Column<string>(nullable: true),
                    MarketingProductId = table.Column<int>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    UserUpdated = table.Column<string>(nullable: true),
                    UserUpdatedId = table.Column<int>(nullable: true),
                    ProductType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductVersions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Deleted = table.Column<bool>(nullable: false),
                    Modification = table.Column<DateTime>(nullable: false),
                    DpcId = table.Column<int>(nullable: false),
                    Slug = table.Column<string>(nullable: true),
                    Version = table.Column<int>(nullable: false),
                    IsLive = table.Column<bool>(nullable: false),
                    Language = table.Column<string>(nullable: true),
                    Format = table.Column<string>(nullable: true),
                    Data = table.Column<string>(nullable: true),
                    Alias = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: false),
                    Hash = table.Column<string>(nullable: true),
                    MarketingProductId = table.Column<int>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    UserUpdated = table.Column<string>(nullable: true),
                    UserUpdatedId = table.Column<int>(nullable: true),
                    ProductType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegionUpdates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Updated = table.Column<DateTime>(nullable: false),
                    RegionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegionUpdates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductRegions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<int>(nullable: false),
                    RegionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRegions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductRegions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductRegionVersions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ProductVersionId = table.Column<int>(nullable: false),
                    RegionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRegionVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductRegionVersions_ProductVersions_ProductVersionId",
                        column: x => x.ProductVersionId,
                        principalTable: "ProductVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductRegions_ProductId",
                table: "ProductRegions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRegionVersions_ProductVersionId",
                table: "ProductRegionVersions",
                column: "ProductVersionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductRegions");

            migrationBuilder.DropTable(
                name: "ProductRegionVersions");

            migrationBuilder.DropTable(
                name: "RegionUpdates");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "ProductVersions");
        }
    }
}
