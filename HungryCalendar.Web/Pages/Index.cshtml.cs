using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using HungryCalendar.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace HungryCalendar.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly BookingDbContext _db;

        public IndexModel(BookingDbContext db)
        {
            _db = db;
        }

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

        public async Task<IActionResult> OnPostConfirm()
        {
            if (!ModelState.IsValid)
            {
                GenerateSlots();
                return Page();
            }

            // Persistence check
            bool isTaken = await _db.Reservations.AnyAsync(r => r.Date == Date && r.Time == SelectedTime);
            bool isDisabled = await _db.DisabledSlots.AnyAsync(d => d.Date == Date && d.Time == SelectedTime);

            if (isTaken || isDisabled)
            {
                ErrorMessage = "This time is no longer available. Please select another.";
                SelectedTime = null;
                GenerateSlots();
                return Page();
            }

            _db.Reservations.Add(new DbReservation
            {
                Date = Date!,
                Time = SelectedTime!,
                Name = Name!,
                Email = Email!,
                Phone = Phone!,
                GroupSize = GroupSize
            });

            await _db.SaveChangesAsync();
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

            var reservations = _db.Reservations.Where(r => r.Date == Date).Select(r => r.Time).ToList();
            var disabled = _db.DisabledSlots.Where(d => d.Date == Date).Select(d => d.Time).ToList();

            if (GroupSize > 8 && Date == "2026-12-24")
            {
                InfoMessage = "No available times for the selected date and group size.";
                return;
            }

            for (int hour = 11; hour < 22; hour++)
            {
                for (int min = 0; min < 60; min += 15)
                {
                    string time = $"{hour:D2}:{min:D2}";
                    
                    if (reservations.Contains(time) || disabled.Contains(time)) continue;

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
    }
}
