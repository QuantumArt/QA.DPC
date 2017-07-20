using System.Collections.Generic;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;
using System;
using System.Collections.Concurrent;
using QA.Core.DPC.QP.Services;

namespace QA.Core.ProductCatalog.Actions.Services
{
	public class ContentServiceAdapter : IContentService
	{
		private readonly ContentService _contentService;
		private readonly ConcurrentDictionary<int, Content> _contentCache = new ConcurrentDictionary<int, Content>();
        private readonly ConcurrentDictionary<int, IEnumerable<Content>> _contentSiteCache = new ConcurrentDictionary<int, IEnumerable<Content>>();
        private readonly string _qpConnString;

		public ContentServiceAdapter(ContentService contentService, IConnectionProvider connectionProvider)
		{
			if (contentService == null)
				throw new ArgumentNullException(nameof( contentService));

			_contentService = contentService;

			_qpConnString = connectionProvider.GetConnection();
		}

        #region IFieldService implementation
        public Content Read(int id)
        {
            return _contentCache.GetOrAdd(id, _contentService.Read);

        }
        public bool Exists(int id)
        {
            return _contentService.Exists(id);
        }

        public IEnumerable<Content> List(int siteId)
        {
            return _contentSiteCache.GetOrAdd(siteId, _contentService.List);
        }

        public QPConnectionScope CreateQpConnectionScope()
		{
			return new QPConnectionScope(_qpConnString);
		}

		#endregion
	}
}
