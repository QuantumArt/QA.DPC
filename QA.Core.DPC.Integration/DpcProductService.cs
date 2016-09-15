using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using QA.Core.DPC.Integration.DAL;
using QA.Core.Service.Interaction;

namespace QA.Core.DPC.Integration
{
    public class DpcProductService : QAServiceBase, IDpcProductService, IDpcService
    {
        private readonly IProductSerializer _serializer;

        public DpcProductService(IProductSerializer serializer)
        {
            _serializer = serializer;
        }

        public ServiceResult<bool> HasProductChanged(int id, string data)
        {
            return Run(new UserContext(), null, () =>
            {
                using (var ctx = new DpcModelDataContext())
                {
                    var p = ctx.Products.FirstOrDefault(x => x.Id == id);
                    if (p?.Hash == null) return true;
                    return !p.Hash.Equals(GetHash(data), StringComparison.Ordinal);
                }
            });
        }

        public ServiceResult<ProductInfo> Parse(string data)
        {
            return Run(new UserContext(), null, () => ParseInternal(data));
        }

        private ProductInfo ParseInternal(string data)
        {
            try
            {
                return _serializer.Deserialize(data);
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        public ServiceResult UpdateProduct(Product product, string data, string userName, int userId)
        {
            return RunAction(new UserContext(), null, () =>
            {
                using (var ctx = new DpcModelDataContext())
                {
                    var p = ctx.Products.FirstOrDefault(x => x.Id == product.Id);
                    var isNew = p == null;
                    if (isNew)
                    {
                        p = new DAL.Product { Created = DateTime.Now };
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

                    p.Id = product.Id;

                    if (isNew)
                        ctx.Products.InsertOnSubmit(p);

                    ctx.SubmitChanges();
                    regionIds.AddRange(p.ProductRegions.Select(x => x.RegionId));
                    foreach (var r in regionIds)
                    {
                        ctx.RegionUpdated(r);
                    }
                }
            });
        }

        public ServiceResult DeleteProduct(int id)
        {
            return RunAction(new UserContext(), null, () =>
            {
                using (var ctx = new DpcModelDataContext())
                {
                    ctx.DeleteProduct(id);
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

        public int[] GetAllProductId(int page, int pageSize)
        {
            using (var ctx = new DpcModelDataContext())
            {
                return ctx.Products
                    .OrderBy(x => x.Id).Skip(page * pageSize).Take(pageSize)
                    .Select(x => x.Id).ToArray();
            }
        }

        public int[] GetLastProductId(int page, int pageSize, DateTime date)
        {
            using (var ctx = new DpcModelDataContext())
            {
                return ctx.Products
                    .Where(x => x.Updated > date)
                    .OrderBy(x => x.Id).Skip(page * pageSize).Take(pageSize)
                    .Select(x => x.Id).ToArray();
            }
        }

        public string GetProduct(int id)
        {
            using (var ctx = new DpcModelDataContext())
            {
                var p = ctx.Products.FirstOrDefault(x => x.Id == id);
                return p != null ? p.Data : "";
            }
        }
    }
}
