using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Threading.Tasks;

namespace HealthCareWebb.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly HttpClient _httpClient;

        // Constructor injection of IHttpClientFactory to create an HttpClient instance.
        public LogoutModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient(); // Create an HttpClient instance using the factory.
        }

        // Handles POST requests to log the user out.
        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            // 1. Call the API's logout endpoint to perform server-side logout.
            // This ensures the server invalidates the user session or token.
            var response = await _httpClient.PostAsync("http://localhost:5148/api/auth/logout", null);

            // 2. Sign out the user locally by clearing the authentication cookie.
            // This removes the user's session on the client side.
            await HttpContext.SignOutAsync("CookieAuth");


            // If no return URL is specified, redirect the user to the login page.
            return RedirectToPage("/Account/Logout");
        }
    }
}
