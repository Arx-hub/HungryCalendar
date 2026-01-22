using Reqnroll;
using Microsoft.Playwright;

namespace HungryCalendar.Tests.Hooks
{
    [Binding]
    public class Hooks
    {
        public static IPlaywright? Playwright;
        public static IBrowser? Browser;
        public static IPage? Page;

        [BeforeTestRun]
        public static async Task BeforeTestRun()
        {
            // Initialize Playwright
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            
            // Launch the browser (headless by default). 
            // Set Headless = false to see the browser UI during debugging.
            Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true 
            });
        }

        [BeforeScenario]
        public async Task BeforeScenario()
        {
            // Create a new context and page for each scenario to ensure isolation
            if (Browser != null)
            {
                var context = await Browser.NewContextAsync();
                Page = await context.NewPageAsync();
            }
        }

        [AfterScenario]
        public async Task AfterScenario()
        {
            // Close the page after each scenario
            if (Page != null)
            {
                await Page.CloseAsync();
            }
        }

        [AfterTestRun]
        public static async Task AfterTestRun()
        {
            // Cleanup
            if (Browser != null)
            {
                await Browser.CloseAsync();
            }
            Playwright?.Dispose();
        }
    }
}
