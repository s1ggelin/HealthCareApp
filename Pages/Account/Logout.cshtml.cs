using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace HealthCareWebb.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LogoutModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGet()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync("http://localhost:5148/api/auth/logout", null);

            if (response.IsSuccessStatusCode)
            {
                await HttpContext.SignOutAsync("CookieAuth");
                HttpContext.Session.Clear();
                return RedirectToPage("/Account/Login");
            }

            return Page(); // Handle error case, e.g., show a message
        }
    }
}
