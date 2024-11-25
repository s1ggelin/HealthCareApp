using HealthCareABApi.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using HealthCareABApi.DTO;
using System.Diagnostics;

public class AppointmentsModel : PageModel
{
    private readonly HttpClient _httpClient;

    public AppointmentsModel(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    public List<AvailableSlotDto> AvailableSlots { get; private set; } = new List<AvailableSlotDto>();

    [BindProperty]
    public BookAppointmentDto AppointmentBooking { get; set; } = new BookAppointmentDto();

    public async Task OnGetAsync()
    {
        var response = await _httpClient.GetAsync("http://localhost:5148/api/availability");
        if (response.IsSuccessStatusCode)
        {
            var availabilities = await response.Content.ReadFromJsonAsync<List<Availability>>();
            if (availabilities != null)
            {
                // Create AvailableSlotDto list that includes AvailabilityId
                AvailableSlots = availabilities
                    .SelectMany(a => a.AvailableSlots.Select(slot => new AvailableSlotDto
                    {
                        AvailabilityId = a.Id, // Add the availability ID
                        Date = slot.Date,
                        CaregiverId = a.CaregiverId
                    }))
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
        if (AppointmentBooking == null || AppointmentBooking.SelectedSlot == default ||
            AppointmentBooking.CaregiverId == 0 || AppointmentBooking.PatientId == 0)
        {
            ModelState.AddModelError(string.Empty, "Invalid booking data.");
            return Page();
        }

        // Step 1: Book the appointment
        var appointment = new Appointment
        {
            PatientId = AppointmentBooking.PatientId,
            CaregiverId = AppointmentBooking.CaregiverId,
            DateTime = AppointmentBooking.SelectedSlot, // Use the exact selected slot without conversion
            Status = AppointmentStatus.Scheduled
        };

        var response = await _httpClient.PostAsJsonAsync("http://localhost:5148/api/appointments", appointment);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to book the appointment. Please try again.");
            return Page();
        }

        // Step 2: Remove the slot from availability using the `availabilityId`
        var updateResponse = await _httpClient.PutAsJsonAsync(
            $"http://localhost:5148/api/availability/{AppointmentBooking.AvailabilityId}/update-slot",
            AppointmentBooking.SelectedSlot // Use the same selected slot value
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