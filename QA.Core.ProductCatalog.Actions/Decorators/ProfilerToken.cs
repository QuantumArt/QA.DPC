using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.ProductCatalog.Actions.Decorators
{
	public class ProfilerToken
	{
		public Stopwatch Timer { get; private set; }
		public string Service { get; set; }
		public string Method { get; set; }
		public string Parameters { get; set; }
		public string Result { get; set; }
		public string FullName
		{
			get { return Service + "." + Method; }
		}

		public ProfilerToken()
		{
			Timer = new Stopwatch();
		}

		public void AddParameters(string format, params object[] args)
		{
			Parameters = format == null ? "" : String.Format(format, args);
		}

		public void AddResult(string format, params object[] args)
		{
			Result = format == null ? "void" : String.Format(format, args);
		}
	}
}
