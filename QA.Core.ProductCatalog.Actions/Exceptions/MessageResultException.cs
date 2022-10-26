using Quantumart.QP8.BLL.Services.DTO;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace QA.Core.ProductCatalog.Actions.Exceptions
{
    [Serializable]
	public class MessageResultException : ProductException
	{
		private const string MessageResultKey = "MessageResult";

		public MessageResult MessageResult { get; private set; }

		public MessageResultException(int productId, string message, MessageResult messageResult)
			: base(productId, message)
		{
			MessageResult = messageResult;
		}

		protected MessageResultException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			MessageResult = (MessageResult)info.GetValue(MessageResultKey, typeof(MessageResult));
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(MessageResultKey, MessageResult, typeof(MessageResult));
			base.GetObjectData(info, context);
		}
	}
}