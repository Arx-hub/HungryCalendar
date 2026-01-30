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
            await Task.Delay(20000);
            
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
            
            // Ensure a clean database state for tests by clearing reservations and disabled slots
            await ClearDisabledSlotsForToday();

            // Seed a default reservation so admin views have something to show
            await SeedDefaultReservation();
        }

        private async Task ClearDisabledSlotsForToday()
        {
            try
            {
                // Try to locate the database used by the running web app (use solution-relative path first)
                var likelyPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "HungryCalendar.Web", "hungrycalendar.db"));
                string? dbPath = null;

                if (File.Exists(likelyPath))
                {
                    dbPath = likelyPath;
                }
                else
                {
                    // Fallback search locations
                    var dbSearchPaths = new[]
                    {
                        "hungrycalendar.db",
                        Path.Combine(Directory.GetCurrentDirectory(), "hungrycalendar.db"),
                        Path.Combine(Directory.GetCurrentDirectory(), "..", "hungrycalendar.db"),
                        Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "hungrycalendar.db"),
                    };

                    foreach (var path in dbSearchPaths)
                    {
                        var fullPath = Path.GetFullPath(path);
                        if (File.Exists(fullPath))
                        {
                            dbPath = fullPath;
                            break;
                        }
                    }
                }

                // Fallback to the absolute path used in Program.cs if previous searches failed
                // Build a list of candidate DB paths to clean (test-run copies and the web app DB)
                var candidates = new List<string>()
                {
                    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "HungryCalendar.Web", "hungrycalendar.db")),
                    Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "hungrycalendar.db")),
                    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "hungrycalendar.db")),
                    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "hungrycalendar.db")),
                    Path.GetFullPath(Path.Combine("c:", "Users", "arxhe", "VSCode", "Github", "School_Projects", "Ohke2026", "HungryCalendar", "HungryCalendar.Web", "hungrycalendar.db"))
                };

                // Deduplicate and only keep existing files
                var existingCandidates = candidates.Distinct().Where(p => File.Exists(p)).ToList();
                if (existingCandidates.Count == 0)
                    return;

                foreach (var candidate in existingCandidates)
                {
                    var options = new DbContextOptionsBuilder<BookingDbContext>()
                        .UseSqlite($"Data Source={candidate}")
                        .Options;

                    using (var db = new BookingDbContext(options))
                    {
                        db.Database.EnsureCreated();

                        // Diagnostic logging to help debug test DB state
                        try
                        {
                            var beforeDisabled = db.DisabledSlots.Count();
                            var beforeReservations = db.Reservations.Count();
                            var logLine = $"[{DateTime.Now:O}] DB Path={candidate} Before: Disabled={beforeDisabled} Reservations={beforeReservations}\\n";
                            File.AppendAllText(Path.Combine(Path.GetTempPath(), "hc_db_cleanup_log.txt"), logLine);

                            // Remove all disabled slots and reservations to ensure a clean test state
                            if (beforeDisabled > 0 || beforeReservations > 0)
                            {
                                db.DisabledSlots.RemoveRange(db.DisabledSlots);
                                db.Reservations.RemoveRange(db.Reservations);
                                await db.SaveChangesAsync();
                            }

                            var afterDisabled = db.DisabledSlots.Count();
                            var afterReservations = db.Reservations.Count();
                            var logLine2 = $"[{DateTime.Now:O}] DB Path={candidate} After: Disabled={afterDisabled} Reservations={afterReservations}\\n";
                            File.AppendAllText(Path.Combine(Path.GetTempPath(), "hc_db_cleanup_log.txt"), logLine2);
                        }
                        catch
                        {
                            // Ignore logging failures
                        }
                    }
                }
            }
            catch
            {
                // Silently fail if database clearing doesn't work - it's not critical for running tests
            }
        }

        private async Task SeedDefaultReservation()
        {
            try
            {
                var candidates = new List<string>()
                {
                    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "HungryCalendar.Web", "hungrycalendar.db")),
                    Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "hungrycalendar.db")),
                    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "hungrycalendar.db")),
                    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "hungrycalendar.db")),
                    Path.GetFullPath(Path.Combine("c:", "Users", "arxhe", "VSCode", "Github", "School_Projects", "Ohke2026", "HungryCalendar", "HungryCalendar.Web", "hungrycalendar.db"))
                };

                var existing = candidates.Distinct().Where(p => File.Exists(p)).ToList();
                if (!existing.Any()) return;

                foreach (var candidate in existing)
                {
                    var options = new DbContextOptionsBuilder<BookingDbContext>()
                        .UseSqlite($"Data Source={candidate}")
                        .Options;

                    using (var db = new BookingDbContext(options))
                    {
                        db.Database.EnsureCreated();

                        // Seed a reservation for today so admin view shows at least one reservation
                        var date = DateTime.Now.ToString("yyyy-MM-dd");
                        if (!db.Reservations.Any(r => r.Date == date && r.Time == "11:00"))
                        {
                            db.Reservations.Add(new DbReservation
                            {
                                Date = date,
                                Time = "11:00",
                                Name = "Seed Test",
                                Email = "seed@test.com",
                                Phone = "+358000000",
                                GroupSize = 2
                            });
                            await db.SaveChangesAsync();

                            var logLine = $"[{DateTime.Now:O}] Seeded reservation in {candidate} for {date} 11:00\\n";
                            File.AppendAllText(Path.Combine(Path.GetTempPath(), "hc_db_cleanup_log.txt"), logLine);
                        }
                    }
                }
            }
            catch
            {
                // ignore
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
