using QA.Core.DPC.QP.Autopublish.Services;

namespace QA.Core.DPC
{
    public class NotificationAutopublishProvider : INotificationAutopublishProvider
    {
        private readonly INotificationService _service;

        public NotificationAutopublishProvider(INotificationService service)
        {
            _service = service;
        }
        public void PushNotifications(int productId, string product, string[] channels, bool isStage, int userId, string userName, string method, string customerCode)
        {
            var item = new NotificationItem
            {
                ProductId = productId,
                Data = product,
                Channels = channels
            };

            _service.PushNotifications(new[] { item }, isStage, userId, userName, method, customerCode);
        }
    }
}
