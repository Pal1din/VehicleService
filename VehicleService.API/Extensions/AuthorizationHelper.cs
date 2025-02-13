namespace VehicleService.API.Extensions;

internal static class AuthorizationHelper
{
    internal static string GetToken(this HttpContext httpContext)
    {
        var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer ")) return string.Empty;
        var token = authHeader["Bearer ".Length..].Trim();
        return token;
    }
}