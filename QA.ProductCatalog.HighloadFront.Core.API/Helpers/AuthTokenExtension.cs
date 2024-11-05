using Microsoft.AspNetCore.Http;

namespace QA.ProductCatalog.HighloadFront.Core.API.Helpers
{
    internal static class AuthTokenExtension
    {
        private const string AuthTokenUser = "AuthTokenUser";

        public static string GetAuthTokenUser(this HttpContext httpContext)
        {
            return httpContext.Items[AuthTokenUser] as string;
        }

        public static void SetAuthTokenUser(this HttpContext httpContext, string authTokenUser)
        {
            httpContext.Items[AuthTokenUser] = authTokenUser;
        }
    }
}