using HealthCareABApi.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using HealthCareABApi.DTO;

public class AppointmentsModel : PageModel
{
    private readonly HttpClient _httpClient;

    public AppointmentsModel(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    public List<AvailableSlotDto> AvailableSlots { get; private set; } = new List<AvailableSlotDto>();

    [BindProperty]
    public DateTime SelectedSlot { get; set; }

    [BindProperty]
    public int CaregiverId { get; set; }

    [BindProperty]
    public int PatientId { get; set; } // Retrieved from localStorage or passed via hidden input

    public async Task OnGetAsync()
    {
        var response = await _httpClient.GetAsync("http://localhost:5148/api/availability");
        if (response.IsSuccessStatusCode)
        {
            var availabilities = await response.Content.ReadFromJsonAsync<List<Availability>>();
            if (availabilities != null)
            {
                AvailableSlots = availabilities
                    .SelectMany(a => a.AvailableSlots.Select(slot => new AvailableSlotDto
                    {
                        Date = slot.Date,
                        CaregiverId = a.CaregiverId // Associate the caregiver ID with the slot
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
        if (SelectedSlot == default || CaregiverId == 0 || PatientId == 0)
        {
            ModelState.AddModelError(string.Empty, "Invalid booking data.");
            return Page();
        }

        // Create the appointment object
        var appointment = new Appointment
        {
            PatientId = PatientId,
            CaregiverId = CaregiverId,
            DateTime = SelectedSlot,
            Status = AppointmentStatus.Scheduled
        };

        // Step 1: Book the appointment
        var response = await _httpClient.PostAsJsonAsync("http://localhost:5148/api/appointments", appointment);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to book the appointment. Please try again.");
            return Page();
        }

        // Step 2: Remove the slot from availability
        var updateResponse = await _httpClient.PutAsJsonAsync(
            $"http://localhost:5148/api/availability/{CaregiverId}/update-slot",
            SelectedSlot
        );

        if (!updateResponse.IsSuccessStatusCode)
        {
            Console.WriteLine("Failed to update availability. Appointment booked, but availability slot still exists.");
        }

        TempData["Message"] = "Appointment booked successfully!";
        return RedirectToPage("/Dashboard/Index");
    }


}