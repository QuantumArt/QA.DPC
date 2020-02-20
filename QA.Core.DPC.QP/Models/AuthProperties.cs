namespace QA.Core.DPC.QP.Models
{
    public class AuthProperties
    {
        public AuthProperties()
        {
            UserId = 1;
        }
        
        public int UserId { get; set; }
        
        public bool UseAuthorization { get; set; }    
    }
}