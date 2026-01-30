# Testing Guide

This project uses **Reqnroll** (SpecFlow) and **Playwright** for automated BDD testing.

## Prerequisites

1.  **Install Playwright Browsers**
    After building the project, you need to install the necessary browsers for Playwright.
    Run the following command in the `HungryCalendar.Tests` directory:
    ```powershell
    pwsh bin/Debug/net9.0/playwright.ps1 install
    ```
    *If `pwsh` is not available, you can use `powershell`.*

    Alternatively, you can install the global tool:
    ```bash
    dotnet tool install --global Microsoft.Playwright.CLI
    playwright install
    ```

## Running Tests

To run the tests, use `dotnet test` from the root or the test directory:

```bash
dotnet test
```

## Test Structure

*   **Features**: Gherkin `.feature` files describing the scenarios.
*   **StepDefinitions**: C# classes that bind the Gherkin steps to code.
*   **Hooks**: `Hooks.cs` handles the Playwright browser lifecycle (launching/closing browser).

## Current Status

All tests are currently marked as `Pending` (`throw new PendingStepException();`).
To implement a test:
1.  Open the corresponding `StepDefinitions` file.
2.  Replace the `PendingStepException` with actual Playwright code (e.g., `await Page.ClickAsync(...)`).
## Race-condition tests

- Some tests intentionally simulate concurrency to validate the application's transaction and availability checks. For example, the double-booking test performs an end-to-end simulation using two Playwright pages so competing users exercise the same server-side code paths instead of manipulating the DB directly.
- If these tests are flaky on CI, try increasing Playwright timeouts or adding diagnostics (logs or DB snapshots) to help investigate.