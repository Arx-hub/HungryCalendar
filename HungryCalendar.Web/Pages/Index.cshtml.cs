using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HungryCalendar.Web.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? Date { get; set; }

        [BindProperty(SupportsGet = true)]
        public int GroupSize { get; set; } = 2;

        [BindProperty]
        public string? SelectedTime { get; set; }

        [BindProperty]
        [Required]
        public string? Name { get; set; }

        [BindProperty]
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [BindProperty]
        [Required]
        [Phone]
        public string? Phone { get; set; }

        public List<string> LunchSlots { get; set; } = new();
        public List<string> AfternoonSlots { get; set; } = new();

        public bool ShowForm => !string.IsNullOrEmpty(SelectedTime) && !BookingSuccess;
        public bool BookingSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? InfoMessage { get; set; }

        private const int SeparationHour = 14; // 2 PM

        public void OnGet()
        {
            if (string.IsNullOrEmpty(Date))
            {
                Date = DateTime.Now.ToString("yyyy-MM-dd");
            }

            GenerateSlots();
        }

        public void OnPostSelectTime(string time)
        {
            SelectedTime = time;
            GenerateSlots();
        }

        public IActionResult OnPostConfirm()
        {
            if (!ModelState.IsValid)
            {
                GenerateSlots();
                return Page();
            }

            // Mock logic: Prevent booking if Name is "Error"
            if (Name?.ToLower() == "error")
            {
                ErrorMessage = "This time is no longer available. Please select another.";
                SelectedTime = null;
                GenerateSlots();
                return Page();
            }

            BookingSuccess = true;
            return Page();
        }

        public IActionResult OnPostBack()
        {
            SelectedTime = null;
            GenerateSlots();
            return Page();
        }

        private void GenerateSlots()
        {
            LunchSlots.Clear();
            AfternoonSlots.Clear();

            // Simulation: No slots for large groups on specific dates
            if (GroupSize > 8 && Date == "2026-12-24")
            {
                InfoMessage = "No available times for the selected date and group size.";
                return;
            }

            for (int hour = 11; hour < 22; hour++)
            {
                for (int min = 0; min < 60; min += 30)
                {
                    string time = $"{hour:D2}:{min:D2}";
                    
                    // Mock Some reserved slots
                    if (IsReserved(time)) continue;

                    if (hour < SeparationHour)
                    {
                        LunchSlots.Add(time);
                    }
                    else
                    {
                        AfternoonSlots.Add(time);
                    }
                }
            }
        }

        private bool IsReserved(string time)
        {
            // Simple deterministic mock
            int hash = (Date?.GetHashCode() ?? 0) ^ time.GetHashCode();
            return (hash % 7 == 0);
        }
    }
}
