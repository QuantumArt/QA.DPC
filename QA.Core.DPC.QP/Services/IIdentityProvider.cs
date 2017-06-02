using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC.QP.Services
{
    public interface IIdentityProvider
    {
        Identity Identity { get; set; }
    }
}
