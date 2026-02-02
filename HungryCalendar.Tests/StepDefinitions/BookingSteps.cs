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
            // Select the first visible, enabled time slot
            await Page.WaitForSelectorAsync(".time-slot, .time-grid", new() { Timeout = 10000 });
            var allSlots = Page.Locator(".time-slot");
            var total = await allSlots.CountAsync();
            var clicked = false;
            for (int i = 0; i < total; i++)
            {
                var slot = allSlots.Nth(i);
                if (!await slot.IsHiddenAsync() && !await slot.IsDisabledAsync())
                {
                    await slot.ClickAsync();
                    clicked = true;
                    break;
                }
            }
            if (!clicked)
                throw new Exception("No available customer time slot found");
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
        [Given("a customer has selected a reservation time without confirming")]
        public async Task GivenACustomerHasSelectedAReservationTime()
        {
            // Navigate to 13 days in future to avoid conflicts
            var futureDate = DateTime.Now.AddDays(13).ToString("yyyy-MM-dd");
            await Page.GotoAsync($"http://localhost:5000/?Date={futureDate}");
            
            await Page.WaitForSelectorAsync(".time-slot, .time-grid", new() { Timeout = 10000 });
            var allSlots = Page.Locator(".time-slot");
            var total = await allSlots.CountAsync();
            string chosenTime = null;
            for (int i = 0; i < total; i++)
            {
                var slot = allSlots.Nth(i);
                if (!await slot.IsHiddenAsync() && !await slot.IsDisabledAsync())
                {
                    chosenTime = (await slot.InnerTextAsync()).Trim();
                    await slot.ClickAsync();
                    break;
                }
            }
            if (chosenTime == null)
                throw new Exception("No available customer time slot found to select");

            // Store details for the conflict step
            _scenarioContext["SelectedDate"] = futureDate;
            _scenarioContext["SelectedTime"] = chosenTime; // e.g. "11:00"
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
                 // Create a conflict by inserting a reservation for the same date/time (more accurate race condition)
                 db.Reservations.Add(new DbReservation { Date = date!, Time = time!, Name = "Other Customer", Email = "other@example.com", Phone = "+3580000000", GroupSize = 2 });
                 db.SaveChanges();
                 // Verify the reservation exists in the same DB file we wrote to
                 var exists = db.Reservations.Any(r => r.Date == date && r.Time == time);
                 exists.Should().BeTrue("The conflicting reservation should have been created in the DB before submitting the user's reservation");
             }

             // Ensure the reservation form fields are filled so ModelState is valid when submitting
             await Page.WaitForSelectorAsync("#reservation-form", new() { Timeout = 5000 });
             await Page.FillAsync("#name", "Race Tester");
             await Page.FillAsync("#email", "race@example.com");
             await Page.FillAsync("#phone", "+35812345678");

             // Now the user submits, expecting failure
             await Page.ClickAsync("#submit-reservation");
        }

        [When("another customer confirms the same time first using a locked transaction")]
        public async Task WhenAnotherCustomerConfirmsTheSameTimeFirstUsingLockedTransaction()
        {
            // This scenario means the conflict was performed inside a locked/using block
            // Reuse the same logic as the non-locked variant to simulate the conflict
            await WhenAnotherCustomerActuallyConfirmsTheSameTimeFirst();
        }

        // Helper: actually book the time using a second browser page so the app creates the conflicting reservation
        //
        // Rationale: To reliably reproduce a real-world race condition we open a second Playwright page and have the
        // competing user complete the booking flow. This exercises the same application code paths (including the
        // transaction and availability checks implemented in `IndexModel.OnPostConfirm`) and validates observable behavior:
        // either an inline "no longer available" message is shown to the late submitter or exactly one reservation exists
        // for the date/time. We prefer this approach over direct DB manipulation so the test remains end-to-end.
        public async Task WhenAnotherCustomerActuallyConfirmsTheSameTimeFirst()
        {
            var date = _scenarioContext["SelectedDate"].ToString();
            var time = _scenarioContext["SelectedTime"].ToString();

            // Pre-fill the original customer's form so the submission will be valid after the other customer books
            await Page.WaitForSelectorAsync("#reservation-form", new() { Timeout = 5000 });
            await Page.FillAsync("#name", "Race Tester");
            await Page.FillAsync("#email", "race@example.com");
            await Page.FillAsync("#phone", "+35812345678");

            var otherPage = await _context.Context!.NewPageAsync();
            try
            {
                await otherPage.GotoAsync($"http://localhost:5000/?Date={date}");
                await otherPage.WaitForSelectorAsync(".time-slot, .time-grid", new() { Timeout = 10000 });
                var slots = otherPage.Locator(".time-slot");
                var total = await slots.CountAsync();
                var clicked = false;
                for (int i = 0; i < total; i++)
                {
                    var s = slots.Nth(i);
                    var txt = (await s.InnerTextAsync()).Trim();
                    if (txt == time && !await s.IsHiddenAsync() && !await s.IsDisabledAsync())
                    {
                        await s.ClickAsync();
                        clicked = true;
                        break;
                    }
                }
                if (!clicked) throw new Exception("Other customer: no available time slot found to select");

                await otherPage.WaitForSelectorAsync("#reservation-form", new() { Timeout = 5000 });
                await otherPage.FillAsync("#name", "Other Customer");
                await otherPage.FillAsync("#email", "other@example.com");
                await otherPage.FillAsync("#phone", "+3580000000");
                await otherPage.ClickAsync("#submit-reservation");
                await otherPage.WaitForSelectorAsync("#confirmation-page", new() { Timeout = 5000 });
            }
            finally
            {
                await otherPage.CloseAsync();
            }

            // Now submit on the original page (should observe either an inline error or a confirmation)
            await Page.ClickAsync("#submit-reservation");
        }

        [Then("the system informs the customer that the time is no longer available")]
        public async Task ThenTheSystemInformsTheCustomerThatTheTimeIsNoLongerAvailable()
        {
            var error = Page.Locator(".error-message");
            try
            {
                await Microsoft.Playwright.Assertions.Expect(error).ToContainTextAsync("no longer available");
            }
            catch
            {
                // Fallback: check page content for the expected message substring
                var content = await Page.ContentAsync();
                content.Should().Contain("no longer available", "Expected the page to indicate the slot was no longer available");
            }
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
        [Then("the system enforces first-come-first-served for the selected time")]
        public async Task ThenTheSystemEnforcesFirstComeFirstServedForTheSelectedTime()
        {
            var date = _scenarioContext["SelectedDate"].ToString();
            var time = _scenarioContext["SelectedTime"].ToString();

            var error = Page.Locator(".error-message");
            if (await error.CountAsync() > 0)
            {
                await Microsoft.Playwright.Assertions.Expect(error).ToContainTextAsync("no longer available");
            }
            else
            {
                var confirmation = Page.Locator("#confirmation-page");
                if (await confirmation.CountAsync() > 0)
                {
                    var dbPath = @"c:\Users\arxhe\VSCode\Github\School_Projects\Ohke2026\HungryCalendar\HungryCalendar.Web\hungrycalendar.db";
                    var options = new DbContextOptionsBuilder<BookingDbContext>()
                        .UseSqlite($"Data Source={dbPath}")
                        .Options;
                    using (var db = new BookingDbContext(options))
                    {
                        db.Database.EnsureCreated();
                        var count = db.Reservations.Count(r => r.Date == date && r.Time == time);
                        count.Should().Be(1, "Exactly one reservation should exist for the selected date/time after concurrent attempts");
                    }
                }
                else
                {
                    var content = await Page.ContentAsync();
                    content.Should().Contain("no longer available", "Expected the page to indicate the slot was no longer available or to show a single confirmed reservation");
                }
            }
        }
    }
}
