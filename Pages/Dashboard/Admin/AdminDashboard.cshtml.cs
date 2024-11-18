using System;
using System.Collections.Generic;
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

        [BindProperty]
        public int CaregiverId { get; set; }

        [BindProperty]
        public List<DateTime> SelectedTimeSlots { get; set; } = new List<DateTime>();

        public List<DateTime> DaysOfWeek { get; private set; } = new List<DateTime>();

        public async Task OnGetAsync()
        {
            Username = User.Identity.Name;

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
        }

        public async Task<IActionResult> OnPostAddAvailabilityAsync()
        {
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
                CaregiverId = CaregiverId,
                AvailableSlots = availableSlots
            };

            // Call the AvailabilityController API to add availability
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7085/api/availability", availability);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to add availability. Please try again.");
                return Page();
            }

            return Page();
        }
    }
}
