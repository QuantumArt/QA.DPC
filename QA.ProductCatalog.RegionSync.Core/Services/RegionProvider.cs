using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Quantumart.QPublishing;
using Quantumart.QPublishing.Database;
using QA.ProductCatalog.RegionSync.Core.Exstensions;
using Quantumart.QPublishing.Info;

namespace QA.ProductCatalog.RegionSync.Core.Services
{
	public class RegionProvider<TModel, TConfig> : IRegionProvider<TModel>
		where TModel : class
		where TConfig : IRegionProviderConfiguration
	{
		#region Query templates
		private const string GetArticlesQuery =
		@"select
	{0}
from
	content_{1}_united
where
	Archive=0";

		private const string GetTopArticlesQuery =
		@"select
	{0}
from
	content_{1}_united
where
	Archive = 0 AND [{2}] IS NULL";
		#endregion

		#region Protected fields
		protected TConfig Configuration { get; private set;}
		protected DBConnector Connector { get; private set;}
		#endregion

		#region Constructor
		public RegionProvider(TConfig configuration)
		{
			Configuration = configuration;
			Connector = new DBConnector(Configuration.ConnectionString);
		}
		#endregion

		#region IRegionProvider implementation
		public virtual TModel[] GetRegions(Dictionary<string, string> fieldMap)
		{
			return GetArticles<TModel>(Configuration.RegionContentId, fieldMap);
		}

		public virtual TModel[] GetTopRegions<TProperty>(Dictionary<string, string> fieldMap, Expression<Func<TModel, TProperty>> parentFieldSelector)
		{
			return GetTopArticles(Configuration.RegionContentId, fieldMap, parentFieldSelector);
		}

		public virtual void DeleteRegions(IEnumerable<int> ids)
		{
			if (ids == null)
			{
				throw new ArgumentNullException("ids");
			}

			foreach (int id in ids)
			{
				Connector.DeleteContentItem(id);
			}
		}

		public virtual void UpdateRegions(IEnumerable<TModel> regions, Dictionary<string, string> fieldMap)
		{
			if (regions == null)
			{
				throw new ArgumentNullException("regions");
			}

			if (fieldMap == null)
			{
				throw new ArgumentNullException("fieldMap");
			}

			if (regions.Any())
			{
				var values = new List<Dictionary<string, string>>();
				var fields = Connector.GetContentAttributeObjects(Configuration.RegionContentId).ToArray();
				var backRelations = fields
					.Where(f => f.BackRelation != null && f.Type == AttributeType.M2ORelation)
					.Select(f => new { f.Name, Value = f.BackRelation.Id, FieldId = f.Id });

				foreach (var region in regions)
				{
					var fieldValues = new Dictionary<string, string>();

					foreach (var property in typeof(TModel).GetProperties())
					{
						string field;
						if (fieldMap.TryGetValue(property.Name, out field))
						{
							fieldValues[field] = property.GetValue(region).ToString();
						}
					}

					foreach (var br in backRelations)
					{
						fieldValues[br.Name] = br.Value.ToString();
					}

					values.Add(fieldValues);
				}

				var fieldIds = (from field in fields
								join name in fieldMap.Values on field.Name equals name
								select field.Id)
								.Union(backRelations.Select(br => br.FieldId))
								.ToArray();
				
				Connector.ImportToContent(Configuration.RegionContentId, values, Configuration.UserId, fieldIds);
			}
		}
		#endregion

		#region Protected methods
		protected virtual T[] GetArticles<T>(int contentId, Dictionary<string, string> fieldMap)
			where T : class
		{
			if (fieldMap == null)
			{
				throw new ArgumentNullException("fieldMap");
			}

			var fields = GetFieldsDescription(fieldMap);
			var query = String.Format(GetArticlesQuery, fields, contentId);
			var articles = Connector.GetRealData(query);
			return articles.ToArray<T>();
		}

		protected virtual TSource[] GetTopArticles<TSource, TProperty>(int contentId, Dictionary<string, string> fieldMap, Expression<Func<TSource, TProperty>> parentFieldSelector)
			where TSource : class
		{
			if (fieldMap == null)
			{
				throw new ArgumentNullException("fieldMap");
			}

			if (parentFieldSelector == null)
			{
				throw new ArgumentNullException("parentFieldSelector");
			}

			string fieldAlias = GetFieldName(parentFieldSelector);
			string fieldName;
			
			if (!fieldMap.TryGetValue(fieldAlias, out fieldName))
			{
				throw new ArgumentException("field map does not contains field selector");
			}

			var fields = GetFieldsDescription(fieldMap);
			var query = String.Format(GetTopArticlesQuery, fields, contentId, fieldName);
			var articles = Connector.GetRealData(query);
			return articles.ToArray<TSource>();
		}

		protected string GetFieldsDescription(Dictionary<string, string> fieldMap)
		{
			return string.Join(", ", fieldMap.Select(e => "[" + e.Value + "] [" + e.Key + "]").ToArray());
		}

		protected string GetFieldName<TSource, TProperty>(Expression<Func<TSource, TProperty>> parentFieldSelector)
			where TSource : class
		{
			var expression = (MemberExpression)parentFieldSelector.Body;
			return expression.Member.Name;

		}
		#endregion
	}
}
