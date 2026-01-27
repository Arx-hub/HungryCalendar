using Reqnroll;
using Microsoft.Playwright;

namespace HungryCalendar.Tests.Hooks
{
    [Binding]
    public class Hooks
    {
        private static IPlaywright? _playwright;
        private static IBrowser? _browser;
        private readonly PlaywrightContext _context;

        public Hooks(PlaywrightContext context)
        {
            _context = context;
        }

        [BeforeTestRun]
        public static async Task BeforeTestRun()
        {
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
        }
    }
}
