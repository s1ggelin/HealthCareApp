using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HealthCareWebb.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public RegisterModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; private set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7191/api/auth/register", new { Username, Password });

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Account/Login");
            }
            else
            {
                ErrorMessage = "Registration failed.";
                return Page();
            }
        }
    }
}
