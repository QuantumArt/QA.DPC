﻿using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace QA.Core.ProductCatalog.Actions.Exceptions
{
    [Serializable]
	public class ProductException : Exception
	{
		private const string ProductIdKey = "ProductId";

		public int ProductId { get; private set; }

		public ProductException(int productId, string message)
			: base(message)
		{
			ProductId = productId;
		}

		public ProductException(int productId, string message, Exception innerException)
			: base(message, innerException)
		{
			ProductId = productId;
		}

		protected ProductException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			ProductId = info.GetInt32(ProductIdKey);
		}

		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			info.AddValue(ProductIdKey, ProductId);
			base.GetObjectData(info, context);
		}
	}
}
