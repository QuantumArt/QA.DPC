using System;
using System.Text;
using Nest;

namespace QA.ProductCatalog.HighloadFront.Elastic.Extensions
{
    public static class ConnectionConfigurationExtensions
    {
        public static TConnectionSettings EnableTrace<TConnectionSettings>(this TConnectionSettings settings, Action<string> write, bool doTrace)
            where TConnectionSettings : ConnectionSettingsBase<TConnectionSettings>
        {
            if (doTrace)
            {
                return settings.OnRequestCompleted(details =>
                {
                    var sb = new StringBuilder();

                    sb.AppendLine("### ES Uri ###");
                    sb.AppendLine(details.HttpMethod.ToString());
                    sb.AppendLine(details.Uri.ToString());
                    sb.AppendLine("### ES REQEUST ###");
                    if (details.RequestBodyInBytes != null) WriteBinary(details.RequestBodyInBytes, sb);
                    sb.AppendLine("### ES RESPONSE ###");
                    if (details.ResponseBodyInBytes != null) WriteBinary(details.ResponseBodyInBytes, sb);

                    write(sb.ToString());
                })
                   .PrettyJson();
            }
            else
            {
                return settings;
            }
        }

        private static void WriteBinary(byte[] data, StringBuilder sb)
        {
            if (data != null && data.Length > 0)
            {
                sb.AppendLine(Encoding.UTF8.GetString(data));
            }
        }
    }
}
