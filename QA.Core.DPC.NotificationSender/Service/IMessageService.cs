using System.Collections.Generic;
using QA.Core.Service.Interaction;

namespace QA.Core.DPC.Service
{
    /// <summary>
    /// сервис для работы с нотификациями
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// получение списка сообщений для отправки
        /// </summary>
        /// <param name="channel">имя канала</param>
        /// <param name="maxCount">максимальный размер пачки для отправки</param>
        /// <returns></returns>
        ServiceResult<List<Message>> GetMessagesToSend(string channel, int maxCount);


        /// <summary>
        /// удаление сообщения из очереди отправки
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ServiceResult RemoveMessage(int id);

    }
}
