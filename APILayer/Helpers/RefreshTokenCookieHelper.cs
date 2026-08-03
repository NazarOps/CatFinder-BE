using Microsoft.AspNetCore.Http;

namespace APILayer.Helpers
{
    public static class RefreshTokenCookieHelper
    {
        public const string CookieName = "catfinder_refresh_token";
        private const string CookiePath = "/api/auth";

        public static void Append(HttpResponse response, string refreshToken, DateTimeOffset expiresAt, bool isHttps)
        {
            response.Cookies.Append(
                CookieName,
                refreshToken,
                BuildOptions(expiresAt, isHttps));
        }

        public static void Clear(HttpResponse response, bool isHttps)
        {
            response.Cookies.Delete(
                CookieName,
                BuildOptions(DateTimeOffset.UnixEpoch, isHttps));
        }

        private static CookieOptions BuildOptions(DateTimeOffset expiresAt, bool isHttps)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                Path = CookiePath,
                Expires = expiresAt,
                SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Secure = isHttps
            };
        }
    }
}
