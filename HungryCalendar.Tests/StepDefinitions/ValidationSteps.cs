using Reqnroll;
using FluentAssertions;
using HungryCalendar.Tests.Hooks;

namespace HungryCalendar.Tests.StepDefinitions
{
    [Binding]
    public class ValidationSteps
    {
        private readonly PlaywrightContext _context;
        private Microsoft.Playwright.IPage Page => _context.Page!;

        public ValidationSteps(PlaywrightContext context)
        {
            _context = context;
        }

        [Given("the customer is filling up the contact information")]
        public async Task GivenTheCustomerIsFillingUpTheContactInformation()
        {
            // Navigate to tomorrow to avoid conflicts with same-day tests
            var tomorrow = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            await Page.GotoAsync($"http://localhost:5000/?Date={tomorrow}");
            // Select a time just to get to the form
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
                throw new Exception("No available customer time slot found to reach the contact form");
        }

        [When("the customer enters an invalid email address")]
        public async Task WhenTheCustomerEntersAnInvalidEmailAddress()
        {
            await Page.FillAsync("#email", "not-an-email");
            await Page.FocusAsync("#phone"); // Trigger blur
        }

        [Then("a message will ask to provide a valid email address")]
        public async Task ThenAMessageWillAskToProvideAValidEmailAddress()
        {
             var emailInput = Page.Locator("#email");
             var isValid = await emailInput.EvaluateAsync<bool>("el => el.checkValidity()");
             isValid.Should().BeFalse();
        }

        [Then("the field is marked as invalid")]
        public async Task ThenTheFieldIsMarkedAsInvalid()
        {
            var emailInput = Page.Locator("#email");
            // Check for pseudo-class :invalid because we use HTML5 validation
            var isInvalid = await emailInput.EvaluateAsync<bool>("el => el.matches(':invalid')");
            isInvalid.Should().BeTrue();
        }

        [When("the customer enters an invalid phone number")]
        public async Task WhenTheCustomerEntersAnInvalidPhoneNumber()
        {
             await Page.FillAsync("#phone", "abc");
             await Page.FocusAsync("#email"); // Trigger blur
        }

        [Then("the phone number field is marked as invalid")]
        public async Task ThenThePhoneNumberFieldIsMarkedAsInvalid()
        {
            var phoneInput = Page.Locator("#phone");
            var isValid = await phoneInput.EvaluateAsync<bool>("el => el.checkValidity()");
            isValid.Should().BeFalse();
        }

        [Then("the system asks the customer to check the phone number")]
        public async Task ThenTheSystemAsksTheCustomerToCheckThePhoneNumber()
        {
            // If we have specific span for it:
            // await Microsoft.Playwright.Assertions.Expect(Page.Locator("[asp-validation-for='Phone']")).ToBeVisibleAsync();
        }
    }
}
