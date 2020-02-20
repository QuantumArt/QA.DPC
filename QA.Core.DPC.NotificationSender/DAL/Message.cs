using System;

namespace QA.Core.DPC.DAL
{
    public class Message
    {
        public int Id { get; set; }
        
        public string Channel { get; set; }
        
        public DateTime Created { get; set; }
        
        public string Data { get; set; }
        
        public string Method { get; set; }
        
        public int UserId { get; set; }
        
        public string UserName { get; set; }
        
        public int DataKey { get; set; }
    }
}