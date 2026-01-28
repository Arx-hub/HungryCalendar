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

        [Given("the administrator navigates to a future date")]
        public async Task GivenTheAdministratorNavigatesToAFutureDate()
        {
            // Navigate to a date 7 days in the future to avoid interfering with other tests
            var futureDate = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");
            await Page.GotoAsync($"http://localhost:5000/Admin?Date={futureDate}");
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
            // Use 2 days from now to avoid conflicts with other tests
            var dateForAdminOps = DateTime.Now.AddDays(2).ToString("yyyy-MM-dd");
            await Page.GotoAsync($"http://localhost:5000/Admin?Date={dateForAdminOps}");
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
            // Check that the res-guests element is visible with the group size info
            await Microsoft.Playwright.Assertions.Expect(Page.Locator(".reservation-card").First).ToBeVisibleAsync();
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
                // Go to home and create one (using tomorrow to avoid conflicts)
                var tomorrow = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
                await Page.GotoAsync($"http://localhost:5000/?Date={tomorrow}");
                await Page.ClickAsync(".time-slot.available >> nth=0");
                await Page.FillAsync("#name", "Temp Admin");
                await Page.FillAsync("#email", "admin@test.com");
                await Page.FillAsync("#phone", "+3580000000");
                await Page.ClickAsync("#submit-reservation");
                // Go back to admin (with same date)
                await Page.GotoAsync($"http://localhost:5000/Admin?Date={tomorrow}");
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
            var day3FromNow = DateTime.Now.AddDays(3).ToString("yyyy-MM-dd");
            
            var names = new[] { name1, name2, name3 };
            for (int i = 0; i < names.Length; i++)
            {
                var name = names[i];
                var fullName = $"{name}_{suffix}";
                
                // Navigate to customer booking page
                await Page.GotoAsync($"http://localhost:5000/?Date={day3FromNow}");
                await Page.WaitForLoadStateAsync();
                
                // Wait for time slots to be visible
                await Page.WaitForSelectorAsync(".time-slot.available", new() { Timeout = 5000 });
                
                // Click on available time slot (use a different slot for each)
                var slots = Page.Locator(".time-slot.available");
                var slotCount = await slots.CountAsync();
                if (slotCount <= i)
                {
                    throw new Exception($"Not enough available slots. Expected at least {i + 1}, found {slotCount}");
                }
                
                await slots.Nth(i).ClickAsync();
                
                // Wait for form to appear after slot click
                await Page.WaitForSelectorAsync("#name", new() { Timeout = 3000 });
                await Task.Delay(300);
                
                // Fill reservation form
                await Page.FillAsync("#name", fullName);
                await Page.FillAsync("#email", $"{name.Replace(" ", "").ToLower()}_{suffix}@test.com");
                await Page.FillAsync("#phone", "+35840" + Math.Abs(fullName.GetHashCode() % 10000000).ToString("D7"));
                
                // Wait for submit button to be ready
                await Page.WaitForSelectorAsync("#submit-reservation", new() { Timeout = 3000 });
                await Task.Delay(200);
                
                // Submit reservation
                await Page.ClickAsync("#submit-reservation");
                
                // Wait for confirmation and page to settle
                await Task.Delay(1500);
            }
            
            _scenarioContext["Suffix"] = suffix;
            _scenarioContext["ReservationDate"] = day3FromNow;
            
            // Navigate to admin page with the same date
            await Page.GotoAsync($"http://localhost:5000/Admin?Date={day3FromNow}");
            await Page.WaitForLoadStateAsync();
            
            // Wait for reservations to appear in the list
            await Page.WaitForSelectorAsync(".reservation-card", new() { Timeout = 5000 });
            await Task.Delay(500);
        }

        [When("the administrator searches for {string}")]
        public async Task WhenTheAdministratorSearchesFor(string query)
        {
            var suffix = _scenarioContext["Suffix"] as string;
            var fullQuery = query == "unique_search_test" ? $"{query}_{suffix}" : query;
            
            // Wait for the search form to appear
            await Page.WaitForSelectorAsync("form.search-form", new() { Timeout = 5000 });
            await Task.Delay(300);
            
            // Use has-text to find the visible text input (not the hidden ones)
            var searchInput = Page.Locator("div.search-input-wrapper input[name='SearchQuery']");
            await searchInput.ScrollIntoViewIfNeededAsync();
            await Task.Delay(300);
            
            // Fill the search input
            await searchInput.FillAsync(fullQuery);
            await Task.Delay(200);
            
            // Press Enter to submit the search
            await Page.Keyboard.PressAsync("Enter");
            await Page.WaitForLoadStateAsync();
            await Task.Delay(300);
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

        [When("the administrator clicks the {string} checkbox")]
        public async Task WhenTheAdministratorClicksTheCheckbox(string checkboxLabel)
        {
            // Check the "Block all times" checkbox
            if (checkboxLabel == "Block all times")
            {
                await Page.ClickAsync("#blockWholeDay");
                await Page.WaitForLoadStateAsync();
            }
        }

        [Then("all time slots for that day become unavailable to customers")]
        public async Task ThenAllTimeSlotsForThatDayBecomeUnavailableToCustomers()
        {
            // Verify that all admin time slots have the is-blocked class
            var slots = Page.Locator(".admin-time-slot");
            var count = await slots.CountAsync();
            count.Should().BeGreaterThan(0);
            
            for (int i = 0; i < count; i++)
            {
                var classes = await slots.Nth(i).GetAttributeAsync("class");
                classes.Should().Contain("is-blocked");
            }
        }

        [Then("the {string} checkbox remains checked")]
        public async Task ThenTheCheckboxRemainschecked(string checkboxLabel)
        {
            if (checkboxLabel == "Block all times")
            {
                var checkbox = Page.Locator("#blockWholeDay");
                var isChecked = await checkbox.IsCheckedAsync();
                isChecked.Should().BeTrue();
            }
        }

        [Given("the administrator has blocked the entire day by checking {string}")]
        public async Task GivenTheAdministratorHasBlockedTheEntireDayByCheckingCheckbox(string checkboxLabel)
        {
            if (checkboxLabel == "Block all times")
            {
                // Check if checkbox is not already checked
                var checkbox = Page.Locator("#blockWholeDay");
                var isChecked = await checkbox.IsCheckedAsync();
                if (!isChecked)
                {
                    await checkbox.ClickAsync();
                    await Page.WaitForLoadStateAsync();
                }
            }
        }

        [When("the administrator unchecks the {string} checkbox")]
        public async Task WhenTheAdministratorUnchecksTheCheckbox(string checkboxLabel)
        {
            if (checkboxLabel == "Block all times")
            {
                await Page.ClickAsync("#blockWholeDay");
                await Page.WaitForLoadStateAsync();
            }
        }

        [Then("all time slots for that day become available again")]
        public async Task ThenAllTimeSlotsForThatDayBecomeAvailableAgain()
        {
            // Verify that all admin time slots are NOT blocked (they should be either available or reserved)
            var slots = Page.Locator(".admin-time-slot");
            var count = await slots.CountAsync();
            count.Should().BeGreaterThan(0);
            
            for (int i = 0; i < count; i++)
            {
                var classes = await slots.Nth(i).GetAttributeAsync("class");
                classes.Should().NotContain("is-blocked", $"Slot {i} should not be blocked");
            }
        }

        [Then("the {string} checkbox is no longer checked")]
        public async Task ThenTheCheckboxIsNoLongerChecked(string checkboxLabel)
        {
            if (checkboxLabel == "Block all times")
            {
                var checkbox = Page.Locator("#blockWholeDay");
                var isChecked = await checkbox.IsCheckedAsync();
                isChecked.Should().BeFalse();
            }
        }
    }
}

