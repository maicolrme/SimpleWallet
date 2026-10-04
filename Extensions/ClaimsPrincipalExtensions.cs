using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace SimpleWallet.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(value, out var userId) ? userId : 0;
    }
}
