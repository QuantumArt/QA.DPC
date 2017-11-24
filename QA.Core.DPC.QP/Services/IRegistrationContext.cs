namespace QA.Core.DPC.QP.Services
{
    public interface IRegistrationContext
    {
        void Register<T>(string key, T value);
    }
}
