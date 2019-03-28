using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace QA.Core.DPC.Front.DAL
{
    
    public partial class DpcModelDataContext : DbContext
    {
        public DpcModelDataContext()
        {
        }

        public DpcModelDataContext(DbContextOptions<DpcModelDataContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductRegion> ProductRegions { get; set; }
        public virtual DbSet<ProductVersion> ProductVersions { get; set; }
        
        public virtual DbSet<ProductRegionVersion> ProductRegionVersions { get; set; } 

        public virtual DbSet<RegionUpdate> RegionUpdates { get; set; }
        
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
            return GetProducts(locator).Include(m => m.ProductRegions).FirstOrDefault(m => id == m.DpcId);
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
            if (string.IsNullOrEmpty(locator.Slug))
            {
                return ProductVersions.Where(m =>
                    m.IsLive == locator.IsLive &&
                    m.Language == locator.Language &&
                    m.Format == locator.Format &&
                    m.Version == locator.Version &&
                    m.Slug.Equals(null) &&
                    m.Modification <= date);
            }
            return ProductVersions.Where(m =>
                m.IsLive == locator.IsLive &&
                m.Language == locator.Language &&
                m.Format == locator.Format &&
                m.Version == locator.Version &&
                m.Slug == locator.Slug &&
                m.Modification <= date);
        }

        public IQueryable<Product> GetProducts(ProductLocator locator)
        {
            if (string.IsNullOrEmpty(locator.Slug))
            {
                return Products.Where(m =>
                    m.IsLive == locator.IsLive &&
                    m.Language == locator.Language &&
                    m.Format == locator.Format &&
                    m.Version == locator.Version &&
                    m.Slug.Equals(null));
            }
            return Products.Where(m =>
                m.IsLive == locator.IsLive &&
                m.Language == locator.Language &&
                m.Format == locator.Format &&
                m.Version == locator.Version &&
                m.Slug == locator.Slug);
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
