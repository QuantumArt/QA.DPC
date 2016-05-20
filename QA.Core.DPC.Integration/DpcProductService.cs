using QA.Core.DPC.Integration.DAL;
using QA.Configuration;
using QA.Core;
using QA.Core.Service.Interaction;
using Quantumart.QPublishing;
using Quantumart.QPublishing.Database;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;

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
            return Run<bool>(new UserContext(), null, () =>
            {
                using (DpcModelDataContext ctx = new DpcModelDataContext())
                {
                    var p = ctx.Products.Where(x => x.Id == id).FirstOrDefault();
                    if (p == null || p.Hash == null) return true;
                    return !p.Hash.Equals(GetHash(data), StringComparison.Ordinal);
                }
            });
        }

        public ServiceResult<ProductInfo> Parse(string data)
        {
            return Run<ProductInfo>(new UserContext(), null, () =>
            {

                return ParseInternal(data);

            });
        }

        private ProductInfo ParseInternal(string data)
        {
            try
            {
				return _serializer.Deserialize(data);
            }
            catch (Exception ex)
            {
            }
            return null;
        }


        public ServiceResult UpdateProduct(Product product, string data, string userName, int userId)
        {
            return RunAction(new UserContext(), null, () =>
            {
                using (DpcModelDataContext ctx = new DpcModelDataContext())
                {
                    var p = ctx.Products.Where(x => x.Id == product.Id).FirstOrDefault();
	                bool isNew = p == null;
                    if (isNew)
                    {
                        p = new DAL.Product();
                        p.Created = DateTime.Now;

                    }

                    if (product.MarketingProduct != null)
                    {
                        p.Alias = product.MarketingProduct.Alias;
                        p.Title = product.MarketingProduct.Title;

                        p.MarketingProductId = product.MarketingProduct.Id;
                    }

	                if (String.IsNullOrEmpty(p.Title))
	                {
		                p.Title = product.Title;
	                }

					if (String.IsNullOrEmpty(p.Alias))
						p.Alias = product.Alias;

                    p.Updated = DateTime.Now;

                    p.Data = data;

                    p.Hash = GetHash(data);
                    p.ProductType = product.ProductType;

					p.UserUpdated = userName;
					p.UserUpdatedId = userId;
                    
                    List<int> regionIds = new List<int>();
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

							foreach (int r in product.Regions.Select(x => x.Id))
							{
								if (!p.ProductRegions.Select(x => x.RegionId).Contains(r))
								{
									ProductRegion pr = new ProductRegion();
									pr.RegionId = r;
									pr.Product = p;
									ctx.ProductRegions.InsertOnSubmit(pr);
								}
							}
						}
						else
						{
							foreach (int r in product.Regions.Select(x => x.Id))
							{
								ProductRegion pr = new ProductRegion();
								pr.RegionId = r;
								pr.Product = p;
								ctx.ProductRegions.InsertOnSubmit(pr);
							}

						}
					}

					p.Id = product.Id;

					if (isNew)
						ctx.Products.InsertOnSubmit(p);

                    ctx.SubmitChanges();
                    regionIds.AddRange(p.ProductRegions.Select(x=>x.RegionId));
                    foreach (int r in regionIds)
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
                using (DpcModelDataContext ctx = new DpcModelDataContext())
                {
					ctx.DeleteProduct(id);
                }
            });
        }

        private string GetHash(string data)
        {
            MD5 alg = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.Unicode.GetBytes(data);
            byte[] hash = alg.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }


        public int[] GetAllProductId(int page, int pageSize)
        {
            using (DpcModelDataContext ctx = new DpcModelDataContext())
            {
                return ctx.Products
                    .OrderBy(x => x.Id).Skip(page * pageSize).Take(pageSize)
                    .Select(x => x.Id).ToArray();
            }
        }

		public int[] GetLastProductId(int page, int pageSize, DateTime date)
		{
			using (DpcModelDataContext ctx = new DpcModelDataContext())
			{
				return ctx.Products
					.Where(x => x.Updated > date)
					.OrderBy(x => x.Id).Skip(page * pageSize).Take(pageSize)
					.Select(x => x.Id).ToArray();
			}
		}

		public string GetProduct(int id)
        {
            using (DpcModelDataContext ctx = new DpcModelDataContext())
            {
                var p =
                    ctx.Products
                    .Where(x => x.Id == id).FirstOrDefault();
                return p != null ? p.Data : "";
            }
        }

    }
}
