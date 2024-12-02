using HealthCareABApi.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using HealthCareABApi.DTO;
using System.Diagnostics;

namespace HealthCareWebb.Pages.Booking
{
    public class BookAppointmentModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public BookAppointmentModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }
        public string Username { get; private set; }

        public string UserId { get; private set; }

        public List<AvailableSlotDto> AvailableSlots { get; private set; } = new List<AvailableSlotDto>();

        [BindProperty]
        public BookAppointmentDto AppointmentBooking { get; set; } = new BookAppointmentDto();


        public async Task OnGetAsync()
        {
            Username = User.Identity.Name;

            UserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            var patientId = int.Parse(UserId);           

            var response = await _httpClient.GetAsync("http://localhost:5148/api/availability");
            if (response.IsSuccessStatusCode)
            {
                var availabilities = await response.Content.ReadFromJsonAsync<List<Availability>>();
                if (availabilities != null)
                {

                    var currentTime = DateTime.UtcNow;

                    AvailableSlots = availabilities
                        .SelectMany(a => a.AvailableSlots.Select(slot => new AvailableSlotDto
                        {
                            AvailabilityId = a.Id, 
                            Date = slot.Date,
                            CaregiverId = a.CaregiverId
                        }))
                        .OrderBy(slot => slot.Date)
                        .Where(slot => slot.Date >= currentTime)
                        .ToList();
                        
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Failed to load available slots.");
            }
        }

        public async Task<IActionResult> OnPostBookAppointmentAsync()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var patientId))
            {
                ModelState.AddModelError(string.Empty, "Invalid or missing UserId claim.");
                Debug.WriteLine("Failed to retrieve UserId from claims.");
                return Page();
            }

            if (AppointmentBooking == null || AppointmentBooking.SelectedSlot == default ||
                AppointmentBooking.CaregiverId == 0)
            {
                ModelState.AddModelError(string.Empty, "Invalid booking data.");
                Debug.WriteLine($"Invalid AppointmentBooking data: CaregiverId={AppointmentBooking.CaregiverId}, SelectedSlot={AppointmentBooking.SelectedSlot}");
                return Page();
            }

            
            var appointment = new Appointment
            {
                PatientId = patientId,
                CaregiverId = AppointmentBooking.CaregiverId,
                DateTime = AppointmentBooking.SelectedSlot, 
                Status = AppointmentStatus.Scheduled
            };

            var response = await _httpClient.PostAsJsonAsync("http://localhost:5148/api/appointments", appointment);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to book the appointment. Please try again.");
                Debug.WriteLine("Failed to book the appointment.");
                return Page();
            }

            
            var updateResponse = await _httpClient.PutAsJsonAsync(
                $"http://localhost:5148/api/availability/{AppointmentBooking.AvailabilityId}/update-slot",
                AppointmentBooking.SelectedSlot 
            );

            if (!updateResponse.IsSuccessStatusCode)
            {
                Debug.WriteLine("Failed to update availability. Appointment booked, but availability slot still exists.");
                Debug.WriteLine($"PUT URL: http://localhost:5148/api/availability/{AppointmentBooking.AvailabilityId}/update-slot");
                Debug.WriteLine($"SelectedSlot (raw): {AppointmentBooking.SelectedSlot}");
            }

            TempData["Message"] = "Appointment booked successfully!";
            return RedirectToPage("/Dashboard/Index");
            
        }
    }
}
