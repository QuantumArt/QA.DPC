using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quantumart.QP8.BLL;

namespace QA.Core.DPC.Loader.Services
{
	public interface IQPService
	{
		QPConnectionScope CreateQpConnectionScope();
	}
}
