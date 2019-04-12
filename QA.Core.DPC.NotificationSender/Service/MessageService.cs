using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using QA.Core.Service.Interaction;
using QA.Core.DPC.DAL;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;

namespace QA.Core.DPC.Service
{
	public class MessageService : QAServiceBase, IMessageService
    {
        private ILogger _logger;
        private readonly IConnectionProvider _provider;

        public MessageService(ILogger logger, IConnectionProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        public ServiceResult<List<Message>> GetMessagesToSend(string channel, int maxCount)
        {
            return RunEnumeration<Message>(new UserContext(), null, () =>
            {
                Throws.IfArgumentNull(channel, _ => channel);
                Throws.IfArgumentNull(maxCount, _ => maxCount);

                List<Message> res;
                    using (var t = new TransactionScope(TransactionScopeOption.Required,
                            new TransactionOptions
                            {
                                IsolationLevel = IsolationLevel.ReadUncommitted
                            }))
                    {
                        var ctx = NotificationsModelDataContext.Create(_provider);
                        res = ctx.Messages
                            .Where(x => x.Channel == channel && x.Created <= DateTime.Now.AddSeconds(-10)) //чтобы не читать данные которые сейчас пишут
                            .OrderBy(x => x.Created)
                            .Take(maxCount)
                            .ToList()
                            .Select(x => new Message { Channel = x.Channel, Created = x.Created, Id = x.Id, Method = x.Method, Xml = x.Data,UserId = x.UserId, UserName = x.UserName, Key = x.DataKey}).ToList();

                    }
                return res;
            });
        }

        public ServiceResult RemoveMessage(int id)
        {
            return RunAction(new UserContext(), null, () =>
            {
                Throws.IfArgumentNull(id, _ => id);
                var ctx = NotificationsModelDataContext.Create(_provider);
                var m = ctx.Messages.FirstOrDefault(x => x.Id == id);
                    
				if (m != null)
                {
                    ctx.Messages.Remove(m);
                    ctx.SaveChanges();
                }
            });
        }

       
    }
}
