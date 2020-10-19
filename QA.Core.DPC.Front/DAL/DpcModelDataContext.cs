﻿using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace QA.Core.DPC.Front.DAL
{
    
    public class DpcModelDataContext : DbContext
    {
        public DpcModelDataContext()
        {
        }

        public DpcModelDataContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductRegion> ProductRegions { get; set; }
        public DbSet<ProductVersion> ProductVersions { get; set; }
        
        public DbSet<ProductRegionVersion> ProductRegionVersions { get; set; } 

        public DbSet<RegionUpdate> RegionUpdates { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<ProductRegion>(entity =>
            {
                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductRegions)
                    .HasForeignKey(d => d.ProductId);
            });
            
            modelBuilder.Entity<ProductRegionVersion>(entity =>
            {
                entity.HasOne(d => d.ProductVersion)
                    .WithMany(p => p.ProductRegionVersions)
                    .HasForeignKey(d => d.ProductVersionId);
            });
        }
        
        public Product GetProduct(ProductLocator locator, int id)
        {
            var p = GetProducts(locator).FirstOrDefault(m => id == m.DpcId);
            p.ProductRegions = GetProductRegions(p).ToList();
            return p;
        }

        public ProductVersion GetProductVersion(ProductLocator locator, int id, DateTime date)
        {
            var productVersion = GetProductVersions(locator, date).OrderByDescending(m => m.Id).FirstOrDefault(m => id == m.DpcId);

            if (productVersion == null)
            {
                return null;
            }
            if (productVersion.Deleted)
            {
                return null;
            }
            else
            {
                return productVersion;
            }
        }

        public IQueryable<ProductVersion> GetProductVersions(ProductLocator locator, DateTime date)
        {
            var productVersions = ProductVersions.Where(m =>
                m.IsLive == locator.IsLive &&
                m.Language == locator.Language &&
                m.Format == locator.Format &&
                m.Version == locator.Version &&
                m.Modification <= date
            );
            
            return (string.IsNullOrEmpty(locator.Slug))
                ? productVersions.Where(m => String.IsNullOrEmpty(m.Slug))
                : productVersions.Where(m => m.Slug == locator.Slug);
        }

        public IQueryable<Product> GetProducts(ProductLocator locator)
        {
            var products = Products.Where(m =>
                m.IsLive == locator.IsLive &&
                m.Language == locator.Language &&
                m.Format == locator.Format &&
                m.Version == locator.Version
            );

            return (string.IsNullOrEmpty(locator.Slug))
                ? products.Where(m => String.IsNullOrEmpty(m.Slug))
                : products.Where(m => m.Slug == locator.Slug);
        }

        public IQueryable<ProductRegion> GetProductRegions(Product p)
        {
            return ProductRegions.Where(n => n.ProductId == p.Id);
        }

        public void FillProduct(ProductLocator locator, Product product)
        {
            product.Format = locator.Format;
            product.IsLive = locator.IsLive;
            product.Language = locator.Language;
            product.Slug = !string.IsNullOrEmpty(locator.Slug) ? locator.Slug : null;
            product.Version = locator.Version;
        }
    }
}
