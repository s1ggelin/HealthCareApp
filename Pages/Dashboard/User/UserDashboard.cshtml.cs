using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthCareABApi.Models;
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

        private readonly HttpClient _httpClient;

        public UserDashboardModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [BindProperty]
        public int PatientId { get; set; }

        public List<Appointment> UpcomingAppointments { get; private set; }

        public async Task<IActionResult> OnGetAsync()
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

            var patientId = PatientId;
            UpcomingAppointments = await GetUpcomingAppointments(patientId);

            return Page();
        }

        private async Task<List<Appointment>> GetUpcomingAppointments(int patientId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<Appointment>>($"http://localhost:5148/api/appointments/patient/{patientId}");

                if (response == null)
                {
                    return new List<Appointment>(); // Returnera en tom lista om inga bokningar finns
                }

                // Filtrera bort bokningar som redan har passerat
                return response.Where(a => a.DateTime > DateTime.Now).ToList();
            }
            catch (Exception ex)
            {
                // Hantera eventuella fel och logga dem
                Console.Error.WriteLine($"Error fetching appointments: {ex.Message}");
                return new List<Appointment>(); // Returnera en tom lista vid fel
            }
        }
    }
}
