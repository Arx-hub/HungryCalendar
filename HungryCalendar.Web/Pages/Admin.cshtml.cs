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

        [BindProperty]
        public string? SelectedTime { get; set; }

        public List<string> LunchSlots { get; set; } = new();
        public List<string> AfternoonSlots { get; set; } = new();
        public List<DbReservation> DateReservations { get; set; } = new();
        public List<string> DisabledTimes { get; set; } = new();

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

        private async Task GenerateSlotsAsync()
        {
            LunchSlots.Clear();
            AfternoonSlots.Clear();
            DateReservations = await _db.Reservations.Where(r => r.Date == Date).ToListAsync();
            DisabledTimes = await _db.DisabledSlots.Where(d => d.Date == Date).Select(d => d.Time).ToListAsync();

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
