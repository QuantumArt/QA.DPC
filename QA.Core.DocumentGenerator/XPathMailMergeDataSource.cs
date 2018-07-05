using System.Collections.Concurrent;
using System.Linq;
using System.Xml.XPath;
using GemBox.Document.MailMerging;

namespace QA.Core.DocumentGenerator
{
    internal class XPathMailMergeDataSource : IMailMergeDataSource
	{
		public XPathMailMergeDataSource(XPathNodeIterator nodeIterator, XPathNavigator navigator)
		{
			_navigator = navigator;

			_nodeIterator = nodeIterator;
		}

		private static readonly ConcurrentDictionary<string, XPathExpression> XPathExpressionsCashe = new ConcurrentDictionary<string, XPathExpression>();  
		private readonly XPathNodeIterator _nodeIterator;
		private readonly XPathNavigator _navigator;

		public bool MoveNext()
		{
			return _nodeIterator.MoveNext();
		}

		public bool TryGetValue(string valueName, out object value)
		{
			bool isRangeName = false;

			if (valueName.StartsWith("x:"))
			{
				isRangeName = true;

				valueName = valueName.Substring(2);
			}

			var xPathExpr = XPathExpressionsCashe.GetOrAdd(valueName, XPathExpression.Compile);

			object exprResult = _navigator.Evaluate(xPathExpr, _nodeIterator);

			value = null;

			if (exprResult is XPathNodeIterator)
			{
				var iterator = (XPathNodeIterator)exprResult;

				if (isRangeName)
					value = new XPathMailMergeDataSource(iterator, _navigator);
				else
					if (iterator.Count > 0)
					{
						value = iterator
							.Cast<XPathNavigator>()
							.Select(x => x.Value)
							.Aggregate((w, n) => w + ", " + n);
					}
			}
			else
				value = exprResult;

			return value != null;
		}

		public string Name
		{
			get { return null; }
		}
	}
}
