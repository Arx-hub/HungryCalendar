using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HungryCalendar.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace HungryCalendar.Web.Pages
{
    public class AdminModel : PageModel
    {
        private readonly BookingDbContext _db;

        public AdminModel(BookingDbContext db)
        {
            _db = db;
        }

        [BindProperty(SupportsGet = true)]
        public string? Date { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty]
        public string? SelectedTime { get; set; }

        public List<string> LunchSlots { get; set; } = new();
        public List<string> AfternoonSlots { get; set; } = new();
        public List<DbReservation> DateReservations { get; set; } = new();
        public List<string> DisabledTimes { get; set; } = new();
        public bool IsWholeDayBlocked { get; set; }

        public bool IsLoggedIn { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public string? Username { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        public async Task OnGetAsync()
        {
            IsLoggedIn = HttpContext.Session.GetString("AdminLoggedIn") == "true";
            if (string.IsNullOrEmpty(Date)) Date = DateTime.Now.ToString("yyyy-MM-dd");
            await GenerateSlotsAsync();
        }

        public async Task<IActionResult> OnPostLoginAsync()
        {
            if (Username == "admin" && Password == "1234")
            {
                HttpContext.Session.SetString("AdminLoggedIn", "true");
                IsLoggedIn = true;
                await GenerateSlotsAsync();
                return Page();
            }

            ErrorMessage = "Invalid credentials";
            return Page();
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Remove("AdminLoggedIn");
            return RedirectToPage("/Admin");
        }

        public async Task<IActionResult> OnPostDeleteReservationAsync(int id)
        {
            var res = await _db.Reservations.FindAsync(id);
            if (res != null)
            {
                _db.Reservations.Remove(res);
                await _db.SaveChangesAsync();
            }
            IsLoggedIn = true;
            await GenerateSlotsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostToggleSlotAsync(string time)
        {
            var existing = await _db.DisabledSlots.FirstOrDefaultAsync(d => d.Date == Date && d.Time == time);
            if (existing != null)
            {
                _db.DisabledSlots.Remove(existing);
            }
            else
            {
                _db.DisabledSlots.Add(new DbDisabledSlot { Date = Date!, Time = time });
            }
            
            await _db.SaveChangesAsync();
            IsLoggedIn = true;
            await GenerateSlotsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostToggleBlockWholeDayAsync()
        {
            IsLoggedIn = HttpContext.Session.GetString("AdminLoggedIn") == "true";
            if (string.IsNullOrEmpty(Date)) 
                Date = DateTime.Now.ToString("yyyy-MM-dd");
            
            // Get all time slots for this date
            var allSlots = GetAllTimeSlots();
            var existingDisabledSlots = await _db.DisabledSlots.Where(d => d.Date == Date).ToListAsync();
            
            // Check if all slots are currently disabled (whole day blocked)
            bool allSlotsDisabled = allSlots.All(slot => existingDisabledSlots.Any(d => d.Time == slot));
            
            if (allSlotsDisabled)
            {
                // Unblock the whole day - remove all disabled slots for this date
                _db.DisabledSlots.RemoveRange(existingDisabledSlots);
            }
            else
            {
                // Block the whole day - add all slots as disabled if not already
                foreach (var slot in allSlots)
                {
                    if (!existingDisabledSlots.Any(d => d.Time == slot))
                    {
                        _db.DisabledSlots.Add(new DbDisabledSlot { Date = Date!, Time = slot });
                    }
                }
            }
            
            await _db.SaveChangesAsync();
            IsLoggedIn = true;
            await GenerateSlotsAsync();
            return Page();
        }

        private List<string> GetAllTimeSlots()
        {
            var slots = new List<string>();
            for (int hour = 11; hour < 22; hour++)
            {
                for (int min = 0; min < 60; min += 15)
                {
                    slots.Add($"{hour:D2}:{min:D2}");
                }
            }
            return slots;
        }

        private async Task GenerateSlotsAsync()
        {
            LunchSlots.Clear();
            AfternoonSlots.Clear();
            
            var query = _db.Reservations.Where(r => r.Date == Date);
            if (!string.IsNullOrEmpty(SearchQuery))
            {
                var searchQueryLower = SearchQuery.ToLower();
                query = query.Where(r => r.Name.ToLower().Contains(searchQueryLower) || 
                                         r.Email.ToLower().Contains(searchQueryLower) || 
                                         r.Phone.ToLower().Contains(searchQueryLower));
            }
            // Sort reservations by time in chronological order
            DateReservations = await query.OrderBy(r => r.Time).ToListAsync();
            
            DisabledTimes = await _db.DisabledSlots.Where(d => d.Date == Date).Select(d => d.Time).ToListAsync();
            
            // Check if whole day is blocked
            var allSlots = GetAllTimeSlots();
            IsWholeDayBlocked = allSlots.All(slot => DisabledTimes.Contains(slot));

            for (int hour = 11; hour < 22; hour++)
            {
                for (int min = 0; min < 60; min += 15)
                {
                    string time = $"{hour:D2}:{min:D2}";
                    if (hour < 14) LunchSlots.Add(time);
                    else AfternoonSlots.Add(time);
                }
            }
        }
    }
}
