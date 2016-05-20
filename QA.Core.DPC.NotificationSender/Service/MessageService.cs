using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using QA.Core.Service.Interaction;

namespace QA.Core.DPC.Service
{
	public class MessageService : QAServiceBase, IMessageService
    {
        ILogger _logger;

        public MessageService(ILogger logger)
        {
            _logger = logger;
        }

        public ServiceResult<List<Message>> GetMessagesToSend(string channel, int maxCount)
        {
            return RunEnumeration<Message>(new UserContext(), null, () =>
            {
                Throws.IfArgumentNull(channel, _ => channel);
                Throws.IfArgumentNull(maxCount, _ => maxCount);

                List<Message> res;

                using (var ctx = new Core.DPC.DAL.NotificationsModelDataContext())
                {
                    using (var t = new TransactionScope(TransactionScopeOption.Required,
                            new TransactionOptions
                            {
                                IsolationLevel = IsolationLevel.ReadUncommitted
                            }))
                    {						
                        res = ctx.Messages
                            .Where(x => x.Channel == channel && x.Created <= DateTime.Now.AddSeconds(-10)) //чтобы не читать данные которые сейчас пишут
                            .OrderBy(x => x.Created)
                            .Take(maxCount)
                            .ToList()
                            .Select(x => new Message { Channel = x.Channel, Created = x.Created, Id = x.Id, Method = x.Method, Xml = x.Data,UserId = x.UserId, UserName = x.UserName, Key = x.DataKey}).ToList();

                    }
                }
                return res;
            });
        }

        public ServiceResult RemoveMessage(int id)
        {
            return RunAction(new UserContext(), null, () =>
            {
                Throws.IfArgumentNull(id, _ => id);

                using (var ctx = new Core.DPC.DAL.NotificationsModelDataContext())
                {
                    var m = ctx.Messages.FirstOrDefault(x => x.Id == id);
                    
					if (m != null)
                    {
                        ctx.Messages.DeleteOnSubmit(m);
                        ctx.SubmitChanges();
                    }
                }
            });
        }

       
    }
}
