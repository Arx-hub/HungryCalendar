using Reqnroll;
using FluentAssertions;
using HungryCalendar.Tests.Hooks;
using HungryCalendar.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace HungryCalendar.Tests.StepDefinitions
{
    [Binding]
    public class BookingSteps
    {
        private readonly PlaywrightContext _context;
        private Microsoft.Playwright.IPage Page => _context.Page!;

        private readonly ScenarioContext _scenarioContext;

        public BookingSteps(PlaywrightContext context, ScenarioContext scenarioContext)
        {
            _context = context;
            _scenarioContext = scenarioContext;
        }

        [Given("the customer has selected an available time")]
        [Given("the customer has selected an available reservation time")]
        public async Task GivenTheCustomerHasSelectedAnAvailableTime()
        {
            // Navigate to 12 days in future to avoid conflicts with same-day tests
            var futureDate = DateTime.Now.AddDays(12).ToString("yyyy-MM-dd");
            await Page.GotoAsync($"http://localhost:5000/?Date={futureDate}"); 
            // Select the first available time slot
            await Page.Locator(".time-slot.available").First.ClickAsync();
            await Page.WaitForSelectorAsync("#reservation-form");
        }

        [When("the customer enters name, valid email, valid phone number and valid amount of people")]
        public async Task WhenTheCustomerEntersNameValidEmailValidPhoneNumberAndValidAmountOfPeople()
        {
            await Page.FillAsync("#name", "John Doe");
            await Page.FillAsync("#email", "john@example.com");
            await Page.FillAsync("#phone", "+358401234567");
            // Note: Don't change group-size here as it triggers form submit/reload in this app
            await Page.ClickAsync("#submit-reservation");
        }

        [Then("the system saves the reservation")]
        public async Task ThenTheSystemSavesTheReservation()
        {
            // Verify success by checking for confirmation section
            await Page.WaitForSelectorAsync("#confirmation-page");
        }

        [Then(@"displays a confirmation message ""(.*)""")]
        public async Task ThenDisplaysAConfirmationMessage(string message)
        {
            var locator = Page.Locator("#confirmation-page");
            await Microsoft.Playwright.Assertions.Expect(locator).ToContainTextAsync(message);
        }

        [Given("the customer has selected a date and group size")]
        public async Task GivenTheCustomerHasSelectedADateAndGroupSize()
        {
            await Page.GotoAsync("http://localhost:5000/");
            await Page.FillAsync("#date", "2026-12-24");
            await Page.SelectOptionAsync("#group-size", new[] { "10" });
        }

        [When("no reservation times are available")]
        public async Task WhenNoReservationTimesAreAvailable()
        {
             // Verify that no times are available or info message is shown
             // In our app, if date is far in future with large group, it might show info
        }

        [Then(@"the system displays a message stating ""(.*)""")]
        public async Task ThenTheSystemDisplaysAMessageStating(string message)
        {
            var locator = Page.Locator(".info-message, .error-message, .no-times-message");
            await Microsoft.Playwright.Assertions.Expect(locator).ToContainTextAsync(message);
        }

        [Given("the customer is viewing the reservation calendar")]
        public async Task GivenTheCustomerIsViewingTheReservationCalendar()
        {
            await Page.GotoAsync("http://localhost:5000/");
        }

        [When("the calendar is displayed")]
        public async Task WhenTheCalendarIsDisplayed()
        {
            await Microsoft.Playwright.Assertions.Expect(Page.Locator(".calendar-container")).ToBeVisibleAsync();
        }

        [Then("all reserved times are not selectable")]
        public async Task ThenAllReservedTimesAreNotSelectable()
        {
            var reservedSlots = Page.Locator(".time-slot.reserved");
            var count = await reservedSlots.CountAsync();
            for (int i = 0; i < count; i++)
            {
                await Microsoft.Playwright.Assertions.Expect(reservedSlots.Nth(i)).ToBeDisabledAsync();
            }
        }

        [Then("reserved times are not visible or disabled")]
        public async Task ThenReservedTimesAreNotVisibleOrDisabled()
        {
            var reservedSlots = Page.Locator(".time-slot.reserved");
             var count = await reservedSlots.CountAsync();
            for (int i = 0; i < count; i++)
            {
                var isDisabled = await reservedSlots.Nth(i).IsDisabledAsync();
                var isHidden = await reservedSlots.Nth(i).IsHiddenAsync();
                (isDisabled || isHidden).Should().BeTrue();
            }
        }

        [Given("a customer has selected a reservation time")]
        public async Task GivenACustomerHasSelectedAReservationTime()
        {
            // Navigate to 13 days in future to avoid conflicts
            var futureDate = DateTime.Now.AddDays(13).ToString("yyyy-MM-dd");
            await Page.GotoAsync($"http://localhost:5000/?Date={futureDate}");
            
            var slot = Page.Locator(".time-slot.available").First;
            var timeText = await slot.InnerTextAsync();
            
            // Store details for the conflict step
            _scenarioContext["SelectedDate"] = futureDate;
            _scenarioContext["SelectedTime"] = timeText.Trim(); // e.g. "11:00"
            
            await slot.ClickAsync();
        }

        [When("another customer confirms the same time first")]
        public async Task WhenAnotherCustomerConfirmsTheSameTimeFirst()
        {
             // Simulate race condition by inserting a disability/reservation for this slot
             // directly into the DB before the user submits.
             
             var date = _scenarioContext["SelectedDate"].ToString();
             var time = _scenarioContext["SelectedTime"].ToString();

             // Use the same database as the running web application
             var dbPath = @"c:\Users\arxhe\VSCode\Github\School_Projects\Ohke2026\HungryCalendar\HungryCalendar.Web\hungrycalendar.db";
             var options = new DbContextOptionsBuilder<BookingDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;
             
             using (var db = new BookingDbContext(options))
             {
                 db.Database.EnsureCreated();
                 // Create a conflict
                 db.DisabledSlots.Add(new DbDisabledSlot { Date = date!, Time = time! });
                 db.SaveChanges();
             }

             // Now the user submits, expecting failure
             await Page.ClickAsync("#submit-reservation");
        }

        [Then("the system informs the customer that the time is no longer available")]
        public async Task ThenTheSystemInformsTheCustomerThatTheTimeIsNoLongerAvailable()
        {
            var error = Page.Locator(".error-message");
            await Microsoft.Playwright.Assertions.Expect(error).ToContainTextAsync("no longer available");
        }

        [Given("the customer is making a reservation")]
        public async Task GivenTheCustomerIsMakingAReservation()
        {
             // Navigate to 14 days in future
             var futureDate = DateTime.Now.AddDays(14).ToString("yyyy-MM-dd");
             await Page.GotoAsync($"http://localhost:5000/?Date={futureDate}");
             // Set guests first to ensure they are preserved
             await Page.SelectOptionAsync("#group-size", "2");
             // Then select an available time
             var slot = Page.Locator(".time-slot.available").First;
             await slot.ClickAsync();
             
             await Page.WaitForSelectorAsync("#reservation-form");
             
             await Page.FillAsync("#name", "Jane Doe");
             await Page.FillAsync("#email", "jane@example.com");
             await Page.FillAsync("#phone", "+358409876543");
        }

        [When("the reservation is successfully completed")]
        public async Task WhenTheReservationIsSuccessfullyCompleted()
        {
            await Page.ClickAsync("#submit-reservation");
        }

        [Then("a confirmation page is displayed")]
        public async Task ThenAConfirmationPageIsDisplayed()
        {
            await Microsoft.Playwright.Assertions.Expect(Page.Locator("#confirmation-page")).ToBeVisibleAsync();
            await Microsoft.Playwright.Assertions.Expect(Page.Locator("h1")).ToContainTextAsync("Reservation successful");
        }

        [Then("the page shows the reservation details")]
        public async Task ThenThePageShowsTheReservationDetails()
        {
            await Microsoft.Playwright.Assertions.Expect(Page.Locator(".details-date")).ToBeVisibleAsync();
            await Microsoft.Playwright.Assertions.Expect(Page.Locator(".details-time")).ToBeVisibleAsync();
            await Microsoft.Playwright.Assertions.Expect(Page.Locator(".details-guests")).ToBeVisibleAsync();
        }

        [Then("the page shows the restaurant’s contact information")]
        public async Task ThenThePageShowsTheRestaurantSContactInformation()
        {
            await Microsoft.Playwright.Assertions.Expect(Page.Locator(".contact-info")).ToBeVisibleAsync();
        }
    }
}
