using Reqnroll;
using FluentAssertions;

namespace HungryCalendar.Tests.StepDefinitions
{
    [Binding]
    public class ValidationSteps
    {
        private Microsoft.Playwright.IPage Page => HungryCalendar.Tests.Hooks.Hooks.Page!;

        [Given("the customer is filling up the contact information")]
        public async Task GivenTheCustomerIsFillingUpTheContactInformation()
        {
            await Page.GotoAsync("http://localhost:5000/");
            // Select a time just to get to the form if necessary
            await Page.ClickAsync(".time-slot.available >> nth=0");
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
             // Checking for HTML5 validation message or custom error text
             var emailInput = Page.Locator("#email");
             // Note: Getting HTML5 validation message requires JS
             var validationMessage = await emailInput.EvaluateAsync<string>("el => el.validationMessage");
             validationMessage.Should().NotBeNullOrEmpty();
        }

        [Then("the field is marked as invalid")]
        public async Task ThenTheFieldIsMarkedAsInvalid()
        {
            var emailInput = Page.Locator("#email");
            // Check for a CSS class like 'is-invalid' or 'error'
            // OR check pseudo-class :invalid (Playwright selector :invalid)
            await Microsoft.Playwright.Assertions.Expect(emailInput).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("invalid|error"));
        }

        [When("the customer enters an invalid phone number")]
        public async Task WhenTheCustomerEntersAnInvalidPhoneNumber()
        {
             await Page.FillAsync("#phone", "+358 00 00 00000"); // As per scenario text "in the format..."
             await Page.FocusAsync("#email"); // Trigger blur
        }

        [Then("the phone number field is marked as invalid")]
        public async Task ThenThePhoneNumberFieldIsMarkedAsInvalid()
        {
            var phoneInput = Page.Locator("#phone");
            await Microsoft.Playwright.Assertions.Expect(phoneInput).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("invalid|error"));
        }

        [Then("the system asks the customer to check the phone number")]
        public async Task ThenTheSystemAsksTheCustomerToCheckThePhoneNumber()
        {
            var errorMsg = Page.Locator(".phone-error-message");
            await Microsoft.Playwright.Assertions.Expect(errorMsg).ToBeVisibleAsync();
        }
    }
}
