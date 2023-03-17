using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using QA.Core.DPC.Front.DAL;
using QA.Core.Logger;
using QA.Core.Service.Interaction;

namespace QA.Core.DPC.Front
{
    public class DpcProductService : QAServiceBase, IDpcProductService, IDpcService
    {
        private readonly DpcModelDataContext _context;
        private readonly IProductSerializerFactory _productSerializerFactory;
        
        public DpcProductService(ILogger logger, DpcModelDataContext context, IProductSerializerFactory productSerializerFactory)
        {
            Logger = logger;
            _context = context;
            _productSerializerFactory = productSerializerFactory;
        }

        public ServiceResult<bool> HasProductChanged(ProductLocator locator, int id, string data)
        {
            return Run(new UserContext(), null, () =>
            {
                var p = _context.GetProduct(locator, id);
                if (p?.Hash == null) return true;
                return !p.Hash.Equals(GetHash(data), StringComparison.Ordinal);
            });
        }

        public ServiceResult<ProductInfo> Parse(ProductLocator locator, string data)
        {
            return Run(new UserContext(), null, () => ParseInternal(locator, data));
        }

        private ProductInfo ParseInternal(ProductLocator locator, string data)
        {
            try
            {
                IProductSerializer serializer = _productSerializerFactory.Resolve(locator.Format);

                return serializer.Deserialize(data);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return null;
        }

     
        public ServiceResult UpdateProduct(ProductLocator locator, Product product, string data, string userName, int userId)
        {
            return RunAction(new UserContext(), null, () =>
            {
                var p = _context.GetProduct(locator, product.Id);
                var isNew = p == null;
                var now = DateTime.Now;
                if (isNew)
                {
                    p = new DAL.Product
                    {
                        Created = now,
                        DpcId = product.Id
                    };
                    _context.FillProduct(locator, p);
                }

                if (product.MarketingProduct != null)
                {
                    p.Alias = product.MarketingProduct.Alias;
                    p.Title = product.MarketingProduct.Title;
                    p.MarketingProductId = product.MarketingProduct.Id;
                }

                if (string.IsNullOrEmpty(p.Title))
                {
                    p.Title = product.Title;
                }

                if (string.IsNullOrEmpty(p.Alias))
                    p.Alias = product.Alias;

                p.Updated = now;

                p.Data = data;

                p.Hash = GetHash(data);
                p.ProductType = product.ProductType;

                p.UserUpdated = userName;
                p.UserUpdatedId = userId;

                var regionIds = new List<int>();
                if (product.Regions != null)
                {
                    if (p.Id != 0)
                    {
                        foreach (var pr in p.ProductRegions)
                        {
                            if (!product.Regions.Select(x => x.Id).Contains(pr.RegionId))
                            {
                                regionIds.Add(pr.RegionId);
                                _context.ProductRegions.Remove(pr);

                            }
                        }

                        foreach (var r in product.Regions.Select(x => x.Id))
                        {
                            if (!p.ProductRegions.Select(x => x.RegionId).Contains(r))
                            {
                                var pr = new ProductRegion
                                {
                                    RegionId = r,
                                    Product = p
                                };

                                _context.ProductRegions.Add(pr);
                            }
                        }
                    }
                    else
                    {
                        foreach (var r in product.Regions.Select(x => x.Id))
                        {
                            var pr = new ProductRegion
                            {
                                RegionId = r,
                                Product = p
                            };

                            _context.ProductRegions.Add(pr);
                        }

                    }
                }

                p.DpcId = product.Id;

                if (isNew)
                {
                    _context.Products.Add(p);
                }

                if (locator.UseProductVersions)
                {
                    UpdateExistingProductVersion(p, product.Regions?.Select(r => r.Id), now);
                }

                _context.SaveChanges();
                regionIds.AddRange(p.ProductRegions.Select(x => x.RegionId));
                foreach (var rid in regionIds)
                {
                    var ru = _context.RegionUpdates.SingleOrDefault(n => n.RegionId == rid) ?? new RegionUpdate() { RegionId = rid };
                    ru.Updated = DateTime.Now;
                    if (ru.Id <= 0)
                    {
                        _context.Add(ru);
                    }
                }
            });
        }

        public ServiceResult DeleteProduct(ProductLocator locator, int id, string data)
        {
            return RunAction(new UserContext(), null, () =>
            {

                var p = _context.GetProduct(locator, id);
                if (p != null)
                {
                    _context.Database.BeginTransaction();

                    var isPg = _context is NpgSqlDpcModelDataContext;
                    var sql = isPg
                        ? $"SELECT * from Products where id = {p.Id} FOR UPDATE "
                        : $"SELECT * from Products with(rowlock, xlock) where id = {p.Id}";
                    _context.Database.ExecuteSqlRaw(sql);
                    
                    foreach (var pr in p.ProductRegions)
                    {
                        _context.Remove(pr);
                    }
                    _context.Remove(p);

                    if (locator.UseProductVersions)
                    {
                        UpdateDeletedProductVersion(p, data);
                    }

                    _context.SaveChanges();
                    _context.Database.CommitTransaction();
                }
            });
        }

        private void UpdateExistingProductVersion(DAL.Product p, IEnumerable<int> regionIds, DateTime modification)
        {
            var pv = MapProductVersion(p, modification);
            pv.Deleted = false;

            _context.ProductVersions.Add(pv);        
        }

        private void UpdateDeletedProductVersion(DAL.Product p, string data)
        {
            var pv = MapProductVersion(p, DateTime.Now);
            pv.Deleted = true;
            pv.Data = data;

            _context.ProductVersions.Add(pv);
        }

        private ProductVersion MapProductVersion(DAL.Product p, DateTime modification)
        {
            return new ProductVersion
            {
                Modification = modification,
                DpcId = p.DpcId,
                Alias = p.Alias,
                Created = p.Created,
                Updated = p.Updated,
                Data = p.Data,
                Hash = p.Hash,
                Format = p.Format,
                IsLive = p.IsLive,
                Language = p.Language,
                MarketingProductId = p.MarketingProductId,
                ProductType = p.ProductType,
                Slug = p.Slug,
                Title = p.Title,
                UserUpdated = p.UserUpdated,
                UserUpdatedId = p.UserUpdatedId,
                Version = p.Version
            };
        }

        private static string GetHash(string data)
        {
            var alg = MD5.Create();
            var inputBytes = Encoding.Unicode.GetBytes(data);
            var hash = alg.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            foreach (var t in hash)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }

        public int[] GetAllProductId(ProductLocator locator, int page, int pageSize)
        {
            return _context.GetProducts(locator)
                .OrderBy(x => x.DpcId).Skip(page * pageSize).Take(pageSize)
                .Select(x => x.DpcId).ToArray();
        }

        public int[] GetLastProductId(ProductLocator locator, int page, int pageSize, DateTime date)
        {
            return _context.GetProducts(locator)
                .Where(x => x.Updated > date)
                .OrderBy(x => x.DpcId).Skip(page * pageSize).Take(pageSize)
                .Select(x => x.DpcId).ToArray();
        }

        public string GetProduct(ProductLocator locator, int id)
        {
            var p = _context.GetProduct(locator, id);
            return p != null ? p.Data : "";
        }

        public ProductData GetProductData(ProductLocator locator, int id)
        {
            ProductData result = null;
            var p = _context.GetProduct(locator, id);
            if (p != null)
            {
                result = new ProductData
                {
                    Product = p.Data,
                    Created = p.Created,
                    Updated = p.Updated
                };
            }
            return result;
        }

        public ProductData GetProductVersionData(ProductLocator locator, int id, DateTime date)
        {
            ProductData result = null;
            var p = _context.GetProductVersion(locator, id, date);
            if (p != null)
            {
                result = new ProductData
                {
                    Product = p.Data,
                    Created = p.Created,
                    Updated = p.Updated
                };
            }
            return result;
        }

        public int[] GetAllProductVersionId(ProductLocator locator, int page, int pageSize, DateTime date)
        {
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadUncommitted }))
            {
                var productVersions = _context.GetProductVersions(locator, date);

                return productVersions
                  .Where(x => !productVersions.Any(y => x.DpcId == y.DpcId && x.Id < y.Id) && !x.Deleted)
                  .OrderBy(x => x.DpcId)
                  .Skip(page * pageSize).Take(pageSize)
                  .Select(x => x.DpcId)
                  .ToArray();
            }
        }
    }
}
