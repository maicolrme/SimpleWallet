using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace SimpleWallet.Controllers;

public class CultureController : Controller
{
    public static readonly string[] SupportedCultures = ["en-US", "es-ES", "it-IT"];

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Set(string culture, string? returnUrl = null)
    {
        if (!SupportedCultures.Contains(culture))
            culture = "en-US";

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return Redirect("/");
    }
}
