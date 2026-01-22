using Reqnroll;
using FluentAssertions;

namespace HungryCalendar.Tests.StepDefinitions
{
    [Binding]
    public class BookingSteps
    {
        // specific page interaction logic
        private Microsoft.Playwright.IPage Page => HungryCalendar.Tests.Hooks.Hooks.Page!;

        [Given("the customer has selected an available time")]
        public async Task GivenTheCustomerHasSelectedAnAvailableTime()
        {
            await Page.GotoAsync("http://localhost:5000/"); 
            // Assume we select the first available 18:00 slot
            await Page.ClickAsync("button.time-slot:has-text('18:00')");
        }

        [When("the customer enters name, valid email, valid phone number and valid amount of people")]
        public async Task WhenTheCustomerEntersNameValidEmailValidPhoneNumberAndValidAmountOfPeople()
        {
            await Page.FillAsync("#name", "John Doe");
            await Page.FillAsync("#email", "john@example.com");
            await Page.FillAsync("#phone", "+358401234567");
            await Page.SelectOptionAsync("#group-size", new[] { "4" }); // Assuming dropdown
            await Page.ClickAsync("#submit-reservation");
        }

        [Then("the system saves the reservation")]
        public async Task ThenTheSystemSavesTheReservation()
        {
            // In a UI test, we verify the result, not the DB save directly usually.
            // We wait for navigation or success API response.
            await Page.WaitForURLAsync("**/confirmation");
        }

        [Then(@"displays a confirmation message ""(.*)""")]
        public async Task ThenDisplaysAConfirmationMessage(string message)
        {
            var locator = Page.Locator(".confirmation-message");
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
             // This step assumes the state is already such that no times are available.
             // We might verify that the list is empty or check a 'No times' element.
             // For the sake of the test flow, we can just assert the UI state.
             await Page.WaitForSelectorAsync(".no-times-message");
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
            await Page.GotoAsync("http://localhost:5000/calendar");
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
            // This covers the "Hidden OR Disabled" requirement
            // We check if there are any enabled slots that are actually reserved (setup data matching required)
            // For now, check if 'reserved' class implies disabled or hidden
            var reservedSlots = Page.Locator(".time-slot.reserved");
            // If they are hidden, count is 0 visible?
            // If disabled:
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
            await Page.GotoAsync("http://localhost:5000/");
            await Page.ClickAsync(".time-slot.available >> nth=0");
        }

        [When("another customer confirms the same time first")]
        public async Task WhenAnotherCustomerConfirmsTheSameTimeFirst()
        {
             // This is hard to simulate in a single-threaded UI test without backend mocking.
             // We will simulate the UI reaction: The user tries to submit, but the backend rejects it.
             // We'll proceed to click submit assuming the backend has changed state.
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
             await GivenTheCustomerHasSelectedAnAvailableTime();
             await Page.FillAsync("#name", "Jane Doe");
             await Page.FillAsync("#email", "jane@example.com");
             await Page.FillAsync("#phone", "+358409876543");
             await Page.SelectOptionAsync("#group-size", new[] { "2" });
        }

        [When("the reservation is successfully completed")]
        public async Task WhenTheReservationIsSuccessfullyCompleted()
        {
            await Page.ClickAsync("#submit-reservation");
            await Page.WaitForURLAsync("**/confirmation");
        }

        [Then("a confirmation page is displayed")]
        public async Task ThenAConfirmationPageIsDisplayed()
        {
            await Microsoft.Playwright.Assertions.Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("confirmation"));
            await Microsoft.Playwright.Assertions.Expect(Page.Locator("h1")).ToContainTextAsync("Confirmation");
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
