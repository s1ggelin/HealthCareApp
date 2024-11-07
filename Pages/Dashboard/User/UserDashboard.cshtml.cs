using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HealthCareWebb.Pages.Dashboard
{
    [Authorize(Policy = "UserOnly")]
    public class UserDashboardModel : PageModel
    {
        public string Username { get; private set; }
        public List<string> UserRoles { get; private set; } = new List<string>();

        public void OnGet()
        {
            Username = User.Identity.Name;

            // Output claims for debugging purposes
            foreach (var claim in User.Claims)
            {
                if (claim.Type == System.Security.Claims.ClaimTypes.Role)
                {
                    UserRoles.Add(claim.Value); // Capture roles
                }
                Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }
        }
    }
}
