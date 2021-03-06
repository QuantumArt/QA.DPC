﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QA.Core.DPC.Front.DAL;

namespace QA.Core.DPC.Front.Migrations.SqlServerMigrations
{
    [DbContext(typeof(SqlServerDpcModelDataContext))]
    partial class SqlServerDpcModelDataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.3-servicing-35854")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("QA.Core.DPC.Front.DAL.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Alias");

                    b.Property<DateTime>("Created");

                    b.Property<string>("Data");

                    b.Property<int>("DpcId");

                    b.Property<string>("Format");

                    b.Property<string>("Hash");

                    b.Property<bool>("IsLive");

                    b.Property<string>("Language");

                    b.Property<int?>("MarketingProductId");

                    b.Property<string>("ProductType");

                    b.Property<string>("Slug");

                    b.Property<string>("Title");

                    b.Property<DateTime>("Updated");

                    b.Property<string>("UserUpdated");

                    b.Property<int?>("UserUpdatedId");

                    b.Property<int>("Version");

                    b.HasKey("Id");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("QA.Core.DPC.Front.DAL.ProductRegion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ProductId");

                    b.Property<int>("RegionId");

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.ToTable("ProductRegions");
                });

            modelBuilder.Entity("QA.Core.DPC.Front.DAL.ProductRegionVersion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ProductVersionId");

                    b.Property<int>("RegionId");

                    b.HasKey("Id");

                    b.HasIndex("ProductVersionId");

                    b.ToTable("ProductRegionVersions");
                });

            modelBuilder.Entity("QA.Core.DPC.Front.DAL.RegionUpdate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("RegionId");

                    b.Property<DateTime>("Updated");

                    b.HasKey("Id");

                    b.ToTable("RegionUpdates");
                });

            modelBuilder.Entity("QA.Core.DPC.Front.ProductVersion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Alias");

                    b.Property<DateTime>("Created");

                    b.Property<string>("Data");

                    b.Property<bool>("Deleted");

                    b.Property<int>("DpcId");

                    b.Property<string>("Format");

                    b.Property<string>("Hash");

                    b.Property<bool>("IsLive");

                    b.Property<string>("Language");

                    b.Property<int?>("MarketingProductId");

                    b.Property<DateTime>("Modification");

                    b.Property<string>("ProductType");

                    b.Property<string>("Slug");

                    b.Property<string>("Title");

                    b.Property<DateTime>("Updated");

                    b.Property<string>("UserUpdated");

                    b.Property<int?>("UserUpdatedId");

                    b.Property<int>("Version");

                    b.HasKey("Id");

                    b.ToTable("ProductVersions");
                });

            modelBuilder.Entity("QA.Core.DPC.Front.DAL.ProductRegion", b =>
                {
                    b.HasOne("QA.Core.DPC.Front.DAL.Product", "Product")
                        .WithMany("ProductRegions")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("QA.Core.DPC.Front.DAL.ProductRegionVersion", b =>
                {
                    b.HasOne("QA.Core.DPC.Front.ProductVersion", "ProductVersion")
                        .WithMany("ProductRegionVersions")
                        .HasForeignKey("ProductVersionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
