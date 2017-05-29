using System.Collections.Generic;
using System.Linq;

namespace QA.ProductCatalog.Infrastructure
{
	public class NotificationSenderConfig
	{
		public NotificationSenderConfig()
		{
			Channels = new List<NotificationChannel>();
		}

		/// <summary>
		/// Интервал проверки новых нотификаций для отправки
		/// </summary>
		public int CheckInterval { get; set; }

		/// <summary>
		/// количество ошибок при отправке перед интервалом ожидания
		/// </summary>
		public int ErrorCountBeforeWait { get; set; }

		/// <summary>
		/// интервал ожидания, когда новых уведомлений не отправляется для этого потребителя
		/// </summary>
		public int WaitIntervalAfterErrors { get; set; }

		/// <summary>
		/// количество обрабатываемых сообщений за раз
		/// </summary>
		public int PackageSize { get; set; }

		/// <summary>
		/// timeout для отправки нотификаций
		/// </summary>
		public int TimeOut { get; set; }

		public List<NotificationChannel> Channels { get; set; }

        public bool IsEqualTo(NotificationSenderConfig config)
        {
            if (config == null)
            {
                return false;
            }

            return
                CheckInterval == config.CheckInterval &&
                ErrorCountBeforeWait == config.ErrorCountBeforeWait &&
                PackageSize == config.PackageSize &&
                TimeOut == config.TimeOut &&
                WaitIntervalAfterErrors == config.WaitIntervalAfterErrors &&
                Channels.SequenceEqual(config.Channels);
        }
    }
}
