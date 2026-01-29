using Reqnroll;
using Microsoft.Playwright;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using HungryCalendar.Web.Data;

namespace HungryCalendar.Tests.Hooks
{
    [Binding]
    public class Hooks
    {
        private static IPlaywright? _playwright;
        private static IBrowser? _browser;
        private readonly PlaywrightContext _context;
        private static Process? _serverProcess;

        public Hooks(PlaywrightContext context)
        {
            _context = context;
        }

        [BeforeTestRun]
        public static async Task BeforeTestRun()
        {
            // Start the web server
            // AppContext.BaseDirectory is the bin\Debug\net9.0 directory
            // We need to go up to the solution root
            var solutionRoot = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..");
            var fullSolutionRoot = Path.GetFullPath(solutionRoot);
            
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --project HungryCalendar.Web --no-build",
                WorkingDirectory = fullSolutionRoot,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            _serverProcess = Process.Start(processInfo);
            
            // Wait for the server to be ready - increased timeout to ensure server starts
            await Task.Delay(10000);
            
            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true 
            });
        }

        [BeforeScenario]
        public async Task BeforeScenario()
        {
            if (_browser != null)
            {
                _context.Context = await _browser.NewContextAsync();
                _context.Page = await _context.Context.NewPageAsync();
            }
            
            // Clear disabled slots for today to ensure tests can find available time slots
            await ClearDisabledSlotsForToday();
        }

        private async Task ClearDisabledSlotsForToday()
        {
            try
            {
                // The database is created in the web app's working directory when the server starts
                // Since we start the server from the test project's parent directory,
                // the database will be in the parent of parent (root of solution) or in HungryCalendar.Web
                var dbSearchPaths = new[]
                {
                    "hungrycalendar.db",
                    Path.Combine(Directory.GetCurrentDirectory(), "hungrycalendar.db"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "hungrycalendar.db"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "hungrycalendar.db"),
                };

                string? dbPath = null;
                foreach (var path in dbSearchPaths)
                {
                    var fullPath = Path.GetFullPath(path);
                    if (File.Exists(fullPath))
                    {
                        dbPath = fullPath;
                        break;
                    }
                }
                
                // If database doesn't exist yet, we can't clear it
                if (string.IsNullOrEmpty(dbPath))
                    return;

                var options = new DbContextOptionsBuilder<BookingDbContext>()
                    .UseSqlite($"Data Source={dbPath}")
                    .Options;

                using (var db = new BookingDbContext(options))
                {
                    var todayDate = DateTime.Now.ToString("yyyy-MM-dd");
                    
                    // Delete all disabled slots and reservations for today
                    var slotsToDelete = db.DisabledSlots.Where(d => d.Date == todayDate).ToList();
                    var reservationsToDelete = db.Reservations.Where(r => r.Date == todayDate).ToList();
                    
                    if (slotsToDelete.Count > 0)
                        db.DisabledSlots.RemoveRange(slotsToDelete);
                    if (reservationsToDelete.Count > 0)
                        db.Reservations.RemoveRange(reservationsToDelete);
                    
                    if (slotsToDelete.Count > 0 || reservationsToDelete.Count > 0)
                        await db.SaveChangesAsync();
                }
            }
            catch
            {
                // Silently fail if database clearing doesn't work - it's not critical
            }
        }

        [AfterScenario]
        public async Task AfterScenario()
        {
            if (_context.Page != null)
            {
                await _context.Page.CloseAsync();
            }
            if (_context.Context != null)
            {
                await _context.Context.CloseAsync();
            }
        }

        [AfterTestRun]
        public static async Task AfterTestRun()
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
            }
            _playwright?.Dispose();
            
            // Kill the server process
            if (_serverProcess != null && !_serverProcess.HasExited)
            {
                _serverProcess.Kill();
                _serverProcess.Dispose();
            }
        }
    }
}
