using System;
using System.Security.Cryptography;
using System.Text;

namespace QA.ProductCatalog.HighloadFront
{
    public class HashProcessor
    {
        public string ComputeHash(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(source);
            }

            using var _shaHelper = SHA256.Create();
            var bytes = _shaHelper.ComputeHash(Encoding.UTF8.GetBytes(source));
            var sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
