using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HealthCareWebb.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; private set; }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            try
            {
                // Create an HttpClient using IHttpClientFactory
                var client = _httpClientFactory.CreateClient();

                // Make the API call
                var response = await client.PostAsJsonAsync("http://localhost:5148/api/auth/login", new
                {
                    username = Username,
                    password = Password
                });

                // Log the raw content for debugging
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content: {content}"); // Log the raw response

                // Check for a successful response
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

                    // Check if result is null or username is empty
                    if (result == null || string.IsNullOrEmpty(result.username))
                    {
                        ErrorMessage = "Invalid response from server.";
                        return Page();
                    }

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, result.username),
                new Claim("UserId", result.userId.ToString())
            };

                    // Add user roles as claims
                    foreach (var role in result.roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role)); // Ensure roles are added as claims
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // Sign in the user
                    await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

                    //TempData["UserId"] = result.userId.ToString();

                    // Redirect based on roles
                    if (result.roles.Contains("Admin"))
                    {
                        return RedirectToPage("/Dashboard/Admin/AdminDashboard");
                    }
                    else if (result.roles.Contains("User"))
                    {
                        return RedirectToPage("/Dashboard/User/UserDashboard"); // Ensure you have a user dashboard
                    }
                    else
                    {
                        // Handle case where role is not recognized
                        ErrorMessage = "User role not recognized.";
                        return Page();
                    }
                }
                else
                {
                    // Log error details
                    ErrorMessage = $"Login failed: {content}";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                return Page();
            }

        }

        private class AuthResponse
        {
            public string username { get; set; } // Match this with API response
            public List<string> roles { get; set; }
            public int userId { get; set; }
        }
    }
}
