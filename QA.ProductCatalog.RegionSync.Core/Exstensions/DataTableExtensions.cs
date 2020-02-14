using System;
using System.Data;
using System.Linq;
using Quantumart.QP8.Utils;

namespace QA.ProductCatalog.RegionSync.Core.Exstensions
{
	public static class DataTableExtensions
	{
		public static T[] ToArray<T>(this DataTable data)
			where T : class
		{
			return data.AsEnumerable().Select(row => Converter.ToModelFromDataRow<T>(row)).ToArray();
		}
	}
}
