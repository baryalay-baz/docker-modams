using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    public class LanguageController : Controller
    {
        public IActionResult SetLanguage(string culture, string? returnUrl = null)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1)
                });

            // Safely return to previous page
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            // Fallback if returnUrl is empty or not local
            return RedirectToAction("Index", "Home", new { area = "Users" });
        }

    }
}
