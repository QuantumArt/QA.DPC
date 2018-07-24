using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace QA.Core.DPC.API.Update
{
    [Serializable]
    public class ProductUpdateConcurrencyException : Exception
    {
        private const string ArticleIdsKey = "ArticleIds";
        private const string CustomMessage = "There are outdated articles";

        public int[] ArticleIds { get; private set; }

        public ProductUpdateConcurrencyException(int[] articleIds)
            : base(CustomMessage)
        {
            ArticleIds = articleIds;
        }

        public ProductUpdateConcurrencyException(int[] articleIds, Exception innerException)
            : base(CustomMessage)
        {
            ArticleIds = articleIds;
        }

        protected ProductUpdateConcurrencyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ArticleIds = (int[])info.GetValue(ArticleIdsKey, typeof(int[]));
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue(ArticleIdsKey, ArticleIds);
            base.GetObjectData(info, context);
        }
    }
}
