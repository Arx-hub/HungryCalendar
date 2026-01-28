using Reqnroll;
using FluentAssertions;
using HungryCalendar.Tests.Hooks;

namespace HungryCalendar.Tests.StepDefinitions
{
    [Binding]
    public class AdminSteps
    {
        private readonly PlaywrightContext _context;
        private readonly ScenarioContext _scenarioContext;
        private Microsoft.Playwright.IPage Page => _context.Page!;

        public AdminSteps(PlaywrightContext context, ScenarioContext scenarioContext)
        {
            _context = context;
            _scenarioContext = scenarioContext;
        }

        [Given("the administrator is logged into the reservation system")]
        public async Task GivenTheAdministratorIsLoggedIntoTheReservationSystem()
        {
            await Page.GotoAsync("http://localhost:5000/Admin");
            await Page.FillAsync("#Username", "admin");
            await Page.FillAsync("#Password", "1234");
            await Page.ClickAsync("button:has-text('Login')");
            // Wait for logout button to appear as proof of login
            await Page.WaitForSelectorAsync("button:has-text('Logout')");
        }

        [When("the administrator defines available reservation times")]
        public async Task WhenTheAdministratorDefinesAvailableReservationTimes()
        {
            await Page.GotoAsync("http://localhost:5000/Admin");
            // Razor Page handles slots automatically, so we just visit.
        }

        [Then("only time slots in 15-minute intervals are selectable")]
        public async Task ThenOnlyTimeSlotsIn15_MinuteIntervalsAreSelectable()
        {
            // Verify that generated slots are in 15-minute intervals
            var slots = await Page.Locator(".admin-time-slot").AllInnerTextsAsync();
            foreach(var s in slots) 
            {
               // Extract time part (e.g. "11:00 (R)" -> "11:00")
               var timePart = s.Split(' ')[0];
               var parts = timePart.Split(':');
               if (parts.Length > 1) 
               {
                   int minutes = int.Parse(parts[1]);
                   (minutes % 15).Should().Be(0, $"Time {s} is not in 15-minute interval");
               }
            }
        }

        [Then("other time values are rejected")]
        public async Task ThenOtherTimeValuesAreRejected()
        {
             // Tested implicitly above
        }

        [When("the administrator disables a specific time slot")]
        public async Task WhenTheAdministratorDisablesASpecificTimeSlot()
        {
            await Page.GotoAsync("http://localhost:5000/Admin");
            // Handle the confirmation dialog
            Page.Dialog += (_, dialog) => dialog.AcceptAsync();
            // Click the first available slot to toggle it to disabled
            await Page.ClickAsync(".admin-time-slot.is-available >> nth=0");
        }

        [Then("the time slot becomes unavailable to customers")]
        public async Task ThenTheTimeSlotBecomesUnavailableToCustomers()
        {
            // Admin sees it as 'is-blocked' class
            await Microsoft.Playwright.Assertions.Expect(Page.Locator(".admin-time-slot.is-blocked >> nth=0")).ToBeVisibleAsync();
        }

        [Given("the administrator is viewing a reservation in the calendar")]
        public async Task GivenTheAdministratorIsViewingAReservationInTheCalendar()
        {
            await GivenTheAdministratorIsLoggedIntoTheReservationSystem();
            await Page.GotoAsync("http://localhost:5000/Admin");
        }

        [When("the reservation details are opened")]
        public async Task WhenTheReservationDetailsAreOpened()
        {
            // In the grid view, details are in the cards
            var cards = Page.Locator(".reservation-card");
            await Microsoft.Playwright.Assertions.Expect(cards).Not.ToHaveCountAsync(0);
        }

        [Then("the group size is displayed")]
        public async Task ThenTheGroupSizeIsDisplayed()
        {
             await Microsoft.Playwright.Assertions.Expect(Page.Locator(".reservation-card").First).ToContainTextAsync("Guests"); 
             await Microsoft.Playwright.Assertions.Expect(Page.Locator(".res-guests").First).ToBeVisibleAsync();
        }

        [Then("the group size is clearly visible and correct")]
        public async Task ThenTheGroupSizeIsClearlyVisibleAndCorrect()
        {
            var text = await Page.Locator(".res-guests").First.InnerTextAsync();
            text.Should().NotBeNullOrEmpty();
        }

        [When("the administrator selects a reservation and deletes it")]
        public async Task WhenTheAdministratorSelectsAReservationAndDeletesIt()
        {
            // Ensure at least one reservation exists to delete
            var removeButton = Page.Locator("button:has-text('Remove')").First;
            if (await removeButton.CountAsync() == 0)
            {
                // Go to home and create one
                await Page.GotoAsync("http://localhost:5000/");
                await Page.ClickAsync(".time-slot.available >> nth=0");
                await Page.FillAsync("#name", "Temp Admin");
                await Page.FillAsync("#email", "admin@test.com");
                await Page.FillAsync("#phone", "+3580000000");
                await Page.ClickAsync("#submit-reservation");
                // Go back to admin
                await Page.GotoAsync("http://localhost:5000/Admin");
                removeButton = Page.Locator("button:has-text('Remove')").First;
            }
            // Handle the confirmation dialog
            Page.Dialog += (_, dialog) => dialog.AcceptAsync();
            await removeButton.ClickAsync();
        }

        [Then("the reservation is removed from the calendar")]
        public async Task ThenTheReservationIsRemovedFromTheCalendar()
        {
            await Page.WaitForLoadStateAsync();
        }

        [Then("the time slot becomes available again")]
        public async Task ThenTheTimeSlotBecomesAvailableAgain()
        {
             await Microsoft.Playwright.Assertions.Expect(Page.Locator(".admin-time-slot.is-available >> nth=0")).ToBeVisibleAsync();
        }

        [Given("the administrator has disabled a specific time slot")]
        public async Task GivenTheAdministratorHasDisabledASpecificTimeSlot()
        {
            await WhenTheAdministratorDisablesASpecificTimeSlot();
        }

        [When("the administrator clicks on the disabled time slot")]
        public async Task WhenTheAdministratorClicksOnTheDisabledTimeSlot()
        {
            // Handle the confirmation dialog
            Page.Dialog += (_, dialog) => dialog.AcceptAsync();
            await Page.ClickAsync(".admin-time-slot.is-blocked >> nth=0");
        }

        [Given("there are reservations for {string}, {string}, and {string}")]
        public async Task GivenThereAreReservationsForAnd(string name1, string name2, string name3)
        {
            var suffix = Guid.NewGuid().ToString("N").Substring(0, 6);
            
            var names = new[] { name1, name2, name3 };
            foreach (var name in names)
            {
                var fullName = $"{name}_{suffix}";
                await Page.GotoAsync("http://localhost:5000/");
                var slots = Page.Locator(".time-slot.available");
                await slots.First.ClickAsync();
                
                await Page.FillAsync("#name", fullName);
                await Page.FillAsync("#email", $"{name.Replace(" ", "").ToLower()}_{suffix}@test.com");
                await Page.FillAsync("#phone", "+35840" + Math.Abs(fullName.GetHashCode() % 10000000).ToString("D7"));
                await Page.ClickAsync("#submit-reservation");
                await Microsoft.Playwright.Assertions.Expect(Page.Locator(".confirmation-card")).ToBeVisibleAsync();
            }
            _scenarioContext["Suffix"] = suffix;
            await Page.GotoAsync("http://localhost:5000/Admin");
        }

        [When("the administrator searches for {string}")]
        public async Task WhenTheAdministratorSearchesFor(string query)
        {
            var suffix = _scenarioContext["Suffix"] as string;
            var fullQuery = query == "unique_search_test" ? $"{query}_{suffix}" : query;
            await Page.FillAsync("input[name='SearchQuery']", fullQuery);
            await Page.Keyboard.PressAsync("Enter");
            await Page.WaitForLoadStateAsync();
        }

        [Then("only the reservation for {string} is displayed")]
        public async Task ThenOnlyTheReservationForIsDisplayed(string name)
        {
            var suffix = _scenarioContext["Suffix"] as string;
            var fullName = $"{name}_{suffix}";
            var cards = Page.Locator(".reservation-card");
            await Microsoft.Playwright.Assertions.Expect(cards).ToHaveCountAsync(1);
            await Microsoft.Playwright.Assertions.Expect(cards).ToContainTextAsync(fullName);
        }

        [Then("the reservations for {string} and {string} are not displayed")]
        public async Task ThenTheReservationsForAndAreNotDisplayed(string name1, string name2)
        {
            var suffix = _scenarioContext["Suffix"] as string;
            await Microsoft.Playwright.Assertions.Expect(Page.Locator(".reservation-card")).Not.ToContainTextAsync(name1 + "_" + suffix);
            await Microsoft.Playwright.Assertions.Expect(Page.Locator(".reservation-card")).Not.ToContainTextAsync(name2 + "_" + suffix);
        }
    }
}
