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
        public List<string> SelectedTimeSlots { get; set; } = new List<string>();

        public void OnGet()
        {
            Username = User.Identity.Name;
        }

        public IActionResult OnPostAddAvailability()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Process the selected time slots
            foreach (var timeSlot in SelectedTimeSlots)
            {
                if (DateTime.TryParse(timeSlot, out DateTime slotDateTime))
                {
                  
                }
            }

            // Save changes and redirect
            return RedirectToPage();
        }
    }
}

//Possibility to select timeslots
//Collect data in an array of objects
//Verify that data is collected properly