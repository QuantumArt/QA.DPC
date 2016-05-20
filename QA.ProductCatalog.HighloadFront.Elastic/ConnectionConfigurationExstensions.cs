using System.Diagnostics;
using System.Text;
using Nest;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public static class ConnectionConfigurationExstensions
    {
        public static TConnectionSettings EnableTrace<TConnectionSettings>(this TConnectionSettings settings)
            where TConnectionSettings : ConnectionSettingsBase<TConnectionSettings>
        {
            return settings.OnRequestCompleted(details =>
               {
                   Write("### ES Uri ###");
                   Write(details.HttpMethod.ToString());
                   Write(details.Uri.ToString());
                   Write("### ES REQEUST ###");
                   if (details.RequestBodyInBytes != null) Write(details.RequestBodyInBytes);
                   Write("### ES RESPONSE ###");
                   if (details.ResponseBodyInBytes != null) Write(details.ResponseBodyInBytes);
               })
               .PrettyJson();
        }

        private static void Write(string text)
        {
            Debug.WriteLine(text);
        }

        private static void Write(byte[] data)
        {
            if (data != null && data.Length > 0)
            {
                Write(Encoding.UTF8.GetString(data));
            }
        }
    }
}
