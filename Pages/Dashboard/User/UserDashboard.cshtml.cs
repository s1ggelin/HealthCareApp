using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public string UserId { get; private set; }

        public List<string> UserRoles { get; private set; } = new List<string>();

        private readonly HttpClient _httpClient;

        public UserDashboardModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [BindProperty]
        public int PatientId { get; set; }

        [BindProperty]
        public int AppointmentId { get; set; }

        public List<Appointment> UpcomingAppointments { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Username = User.Identity.Name;

                        UserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;


            // Output claims for debugging purposes
            foreach (var claim in User.Claims)
            {
                if (claim.Type == System.Security.Claims.ClaimTypes.Role)
                {
                    UserRoles.Add(claim.Value); // Capture roles
                }
                Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }

            var patientId = int.Parse(UserId);
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

        public async Task<IActionResult> OnPostDeleteAppointmentAsync(int appointmentId)
        {
            try
            {
                if (appointmentId == 0)
                {
                    ModelState.AddModelError(string.Empty, "Invalid appointment ID.");
                    Debug.WriteLine("Appointment ID:" + appointmentId);
                    return Page();
                }

                Debug.WriteLine($"Attempting to delete appointment with ID: {appointmentId} at {DateTime.Now}");

                var response = await _httpClient.DeleteAsync($"http://localhost:5148/api/appointments/{appointmentId}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Bokningen har avbokats.";
                }
                else
                {
                    TempData["Errormessage"] = $"Kunde inte avboka bokningen. Fel: {await response.Content.ReadAsStringAsync()}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ett fel uppstod vid avbokning: {ex.Message}";
            }

            return RedirectToPage("/Dashboard/User/UserDashboard");
        }
    }
}
