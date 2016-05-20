﻿using QA.Core.ProductCatalog.Actions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.ProductCatalog.Actions.Tests.Fakes
{
	public sealed class TransactionFake : ITransaction
	{
		public bool IsDispossed { get; private set; }
		public bool IsCommited { get; private set; }

		public TransactionFake()
		{
			IsDispossed = false;
			IsCommited = false;
		}

		#region ITransaction implementation
		public void Commit()
		{
			IsCommited = true;
		}

		public void Dispose()
		{
			IsDispossed = true;
		}
		#endregion
	}
}
