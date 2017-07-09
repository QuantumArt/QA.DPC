﻿namespace QA.Core.DPC.QP.Autopublish.Services
{
    public interface INotificationAutopublishProvider
    {
        void PushNotifications(int productId, string product, string[] channels, bool isStage, int userId, string userName, string method, string customerCode);
    }
}