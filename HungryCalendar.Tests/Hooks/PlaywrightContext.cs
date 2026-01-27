using Microsoft.Playwright;

namespace HungryCalendar.Tests.Hooks
{
    public class PlaywrightContext
    {
        public IPage? Page { get; set; }
        public IBrowserContext? Context { get; set; }
    }
}
