using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HealthCareABApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HealthCareWebb.Pages.Dashboard
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminDashboardModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public AdminDashboardModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public string Username { get; private set; }

        public string UserId { get; private set; }

        [BindProperty]
        public int AppointmentId { get; set; }

        [BindProperty]
        public int CaregiverId { get; set; }

        [BindProperty]
        public List<DateTime> SelectedTimeSlots { get; set; } = new List<DateTime>();

        public List<DateTime> DaysOfWeek { get; private set; } = new List<DateTime>();

        public List<Appointment> UpcomingAppointments { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Username = User.Identity.Name;

            UserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            Console.WriteLine("UserId " + UserId);

            var startDate = DateTime.Today;
            var daysOfWeek = new List<DateTime>();

            while (daysOfWeek.Count < 5)
            {
                if (startDate.DayOfWeek != DayOfWeek.Saturday && startDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    daysOfWeek.Add(startDate);
                }
                startDate = startDate.AddDays(1);
            }
            DaysOfWeek = daysOfWeek;

            var caregiverId = int.Parse(UserId);
            UpcomingAppointments = await GetUpcomingAppointments(caregiverId);

            return Page();
        }

        public async Task<List<Appointment>> GetUpcomingAppointments(int caregiverId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<Appointment>>($"http://localhost:5148/api/appointments/caregiver/{caregiverId}");

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

        public async Task<IActionResult> OnPostAddAvailabilityAsync()
        {
            UserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            if (string.IsNullOrEmpty(UserId) || !int.TryParse(UserId, out var caregiverId))
            {
                ModelState.AddModelError(string.Empty, "Invalid or missing UserId claim.");
                Debug.WriteLine("Failed to retrieve UserId from claims.");
                return Page();
            }

            if (SelectedTimeSlots == null || SelectedTimeSlots.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Please add at least one available slot.");
                return Page();
            }

            // Convert the selected times into AvailableSlot objects
            var availableSlots = SelectedTimeSlots.ConvertAll(slot => new AvailableSlot { Date = slot });

            // Create the availability object
            var availability = new Availability
            {
                CaregiverId = caregiverId,
                AvailableSlots = availableSlots
            };

            // Call the AvailabilityController API to add availability
            var response = await _httpClient.PostAsJsonAsync("http://localhost:5148/api/availability", availability);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to add availability. Please try again.");
                return Page();
            }

            return Page();
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

            return RedirectToPage("/Dashboard/Admin/AdminDashboard");
        }
    }
}