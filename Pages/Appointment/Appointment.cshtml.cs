using HealthCareABApi.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

public class AppointmentsModel : PageModel
{
	private readonly HttpClient _httpClient;

	public AppointmentsModel(IHttpClientFactory httpClientFactory)
	{
		_httpClient = httpClientFactory.CreateClient();
	}

	public List<AvailableSlot> AvailableSlots { get; private set; } = new List<AvailableSlot>();

	[BindProperty]
	public int CaregiverId { get; set; }

	[BindProperty]
	public DateTime SelectedSlot { get; set; }

	[BindProperty]
	public int UserId { get; set; } // Bind the user ID from the form

	public async Task OnGetAsync(int caregiverId)
	{
		CaregiverId = caregiverId;

		var response = await _httpClient.GetAsync($"http://localhost:5148/api/availability/caregiver/{CaregiverId}");
		if (response.IsSuccessStatusCode)
		{
			AvailableSlots = await response.Content.ReadFromJsonAsync<List<AvailableSlot>>();
		}
	}

	public async Task<IActionResult> OnPostBookAppointmentAsync()
	{
		if (SelectedSlot == DateTime.MinValue)
		{
			ModelState.AddModelError(string.Empty, "Please select a valid slot.");
			return Page();
		}

		if (UserId == 0)
		{
			ModelState.AddModelError(string.Empty, "User ID is missing. Please log in again.");
			return Page();
		}

		var appointment = new Appointment
		{
			CaregiverId = CaregiverId,
			PatientId = UserId, // Use the user ID from the form
			DateTime = SelectedSlot,
			Status = AppointmentStatus.Scheduled
		};

		var response = await _httpClient.PostAsJsonAsync("http://localhost:5148/api/appointments", appointment);

		if (!response.IsSuccessStatusCode)
		{
			ModelState.AddModelError(string.Empty, "Failed to book the appointment. Please try again.");
			return Page();
		}

		return RedirectToPage("/Appointment/Confirmation");
	}
}
