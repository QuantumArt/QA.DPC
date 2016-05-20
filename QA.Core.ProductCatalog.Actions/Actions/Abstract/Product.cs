using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL;

namespace QA.Core.ProductCatalog.Actions.Actions.Abstract
{
	public class Product<T> where T : struct
	{
		public Article Article { get; set; }
		public bool IsProcessed { get; set; }
		public Dictionary<int, T> FieldModes { get; private set; }
		public Dictionary<int, bool> BackwardMap { get; private set; }
		public List<FieldValue> BackwardFieldValues { get; private set; }

		public Product(Article article, Dictionary<int, T> fieldModes, List<FieldValue> backwardFieldValues)
		{			
			if (article == null)
				throw new ArgumentNullException("article");

			if (fieldModes == null)
				throw new ArgumentNullException("fieldModes");

			if (backwardFieldValues == null)
				throw new ArgumentNullException("backwardFieldValues");

			Article = article;
			FieldModes = fieldModes;
			BackwardMap = article.FieldValues.ToDictionary(fv => fv.Field.Id, fv => false);
			BackwardFieldValues = backwardFieldValues;
			IsProcessed = false;
		}

		public T? GetCloningMode(Quantumart.QP8.BLL.Field field)
		{
			if (field == null)
				throw new ArgumentNullException("field");

			if (FieldModes.ContainsKey(field.Id))
			{
				return FieldModes[field.Id];
			}
			else
			{
				return null;
			}
		}
	}
}