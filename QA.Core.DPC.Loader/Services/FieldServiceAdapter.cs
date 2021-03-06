﻿using System.Collections.Generic;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;
using System;
using System.Collections.Concurrent;
using QA.Core.DPC.QP.Services;
using Quantumart.QP8.Constants;
using QA.Core.DPC.QP.Models;

namespace QA.Core.ProductCatalog.Actions.Services
{
	public class FieldServiceAdapter : IFieldService
	{
		private readonly FieldService _fieldService;
		private readonly ConcurrentDictionary<int, Field> _fieldMap;
		private readonly Customer _customer;

		public FieldServiceAdapter(FieldService fieldService, IConnectionProvider connectionProvider)
		{
			if (fieldService == null)
				throw new ArgumentNullException(nameof(fieldService));

			_fieldService = fieldService;
			_fieldMap = new ConcurrentDictionary<int, Field>();

			_customer = connectionProvider.GetCustomer();
		}

		#region IFieldService implementation
		public Field Read(int id)
		{
			return _fieldMap.GetOrAdd(id, _fieldService.Read);
		}

		public IEnumerable<Field> List(int contentId)
		{
			return _fieldService.List(contentId);
		}

		public IEnumerable<Field> ListRelated(int contentId)
		{
			return _fieldService.ListRelated(contentId);
		}

		public QPConnectionScope CreateQpConnectionScope()
		{
			return new QPConnectionScope(_customer.ConnectionString, (DatabaseType)_customer.DatabaseType);
		}

		#endregion
	}
}
