using System;

namespace QA.Core.DPC.Service
{
    /// <summary>
    /// сообщение для отправки
    /// </summary>
    public class Message
    {
        public int Id { get; set; }

        public DateTime Created { get; set; }

        public string Channel { get; set; }

        public string Xml { get; set; }

		public int Key { get; set; }

        public string Method { get; set; }

		public int UserId { get; set; }

		public string UserName { get; set; }
    }
}
