using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using QA.Core.DPC.Front.DAL;
using QA.Core.Service.Interaction;
using Quantumart.QPublishing.Database;

namespace QA.Core.DPC.Front
{
    public class DpcProductService : QAServiceBase, IDpcProductService, IDpcService
    {

        public ServiceResult<bool> HasProductChanged(ProductLocator locator, int id, string data)
        {
            return Run(new UserContext(), null, () =>
            {
                using (var ctx = new DpcModelDataContext(locator.GetConnectionString()))
                {
                    var p = ctx.GetProduct(locator, id);
                    if (p?.Hash == null) return true;
                    return !p.Hash.Equals(GetHash(data), StringComparison.Ordinal);
                }
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
                return locator.GetSerialiser().Deserialize(data);
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        public ServiceResult UpdateProduct(ProductLocator locator, Product product, string data, string userName, int userId)
        {
            return RunAction(new UserContext(), null, () =>
            {
                using (var ctx = new DpcModelDataContext(locator.GetConnectionString()))
                {
                    var p = ctx.GetProduct(locator, product.Id);
                    var isNew = p == null;
                    if (isNew)
                    {
                        p = new DAL.Product
                        {
                            Created = DateTime.Now,
                            DpcId = product.Id
                        };
                        ctx.FillProduct(locator, p);
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

                    p.Updated = DateTime.Now;

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
                                    ctx.ProductRegions.DeleteOnSubmit(pr);

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

                                    ctx.ProductRegions.InsertOnSubmit(pr);
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

                                ctx.ProductRegions.InsertOnSubmit(pr);
                            }

                        }
                    }

                    p.DpcId = product.Id;

                    if (isNew)
                    {
                        ctx.Products.InsertOnSubmit(p);
                    }

                    ctx.SubmitChanges();
                    regionIds.AddRange(p.ProductRegions.Select(x => x.RegionId));
                    foreach (var r in regionIds)
                    {
                        ctx.RegionUpdated(r);
                    }
                }
            });
        }

        public ServiceResult DeleteProduct(ProductLocator locator, int id)
        {
            return RunAction(new UserContext(), null, () =>
            {
                using (var ctx = new DpcModelDataContext(locator.GetConnectionString()))
                {
                    var p = ctx.GetProduct(locator, id);
                    if (p != null)
                    {
                        ctx.DeleteProduct(p.Id);
                    }
                }
            });
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
            using (var ctx = new DpcModelDataContext(locator.GetConnectionString()))
            {
                return ctx.GetProducts(locator)
                    .OrderBy(x => x.DpcId).Skip(page * pageSize).Take(pageSize)
                    .Select(x => x.DpcId).ToArray();
            }
        }

        public int[] GetLastProductId(ProductLocator locator, int page, int pageSize, DateTime date)
        {
            using (var ctx = new DpcModelDataContext(locator.GetConnectionString()))
            {
                return ctx.GetProducts(locator)
                    .Where(x => x.Updated > date)
                    .OrderBy(x => x.DpcId).Skip(page * pageSize).Take(pageSize)
                    .Select(x => x.DpcId).ToArray();
            }
        }

        public string GetProduct(ProductLocator locator, int id)
        {
            using (var ctx = new DpcModelDataContext(locator.GetConnectionString()))
            {
                var p = ctx.GetProduct(locator, id);
                return p != null ? p.Data : "";
            }
        }

        public ProductData GetProductData(ProductLocator locator, int id)
        {
            ProductData result = null;
            using (var ctx = new DpcModelDataContext(locator.GetConnectionString()))
            {
                var p = ctx.GetProduct(locator, id);
                if (p != null)
                {
                    result = new ProductData
                    {
                        Product = p.Data,
                        Created = p.Created,
                        Updated = p.Updated
                    };
                }
            }
            return result;
        }
    }
}
