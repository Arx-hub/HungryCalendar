# HungryCalendar

HungryCalendar is a modern web application designed for restaurant table reservations. It provides a seamless experience for customers to book tables and a robust management interface for administrators to control availability and view reservations.

## Table of Contents
1. [Introduction](#introduction)
2. [Key Features](#key-features)
3. [Technical Stack](#technical-stack)
4. [Development Timeline](#development-timeline)
5. [Testing Strategy](#testing-strategy)
6. [Getting Started](#getting-started)

---

## Introduction
The project was developed as part of the Ohke 2026 school project. The goal was to create a functional restaurant booking system using modern web technologies and AI-assisted development (LLM agents).

---

## Key Features

### Customer Features
- **Time Selection**: Choose available time slots in 15-minute intervals.
- **Reservation Forms**: Enter contact details (Name, Email, Phone) and group size.
- **Dynamic Calendar**: View only available times; reserved or blocked times are automatically hidden or disabled.
- **Concurrency Handling**: Implements a "First-Come-First-Serve" policy using locked transactions to prevent double bookings.
- **Confirmation Page**: Post-booking summary with restaurant contact details.

### Administrator Features
- **Dashboard**: View all current reservations with customer details and group sizes.
- **Availability Management**: Block or unblock specific 15-minute time slots.
- **Batch Actions**: "Select Multiple" mode to block/unblock several slots at once or block an entire day.
- **Search**: Quickly find specific reservations by name or contact info.
- **Confirmation Dialogs**: Safety checks when modifying schedules to prevent accidental changes.

---

## Technical Stack
- **Framework**: ASP.NET Core 9.0 (Razor Pages)
- **Database**: SQLite (managed with Entity Framework Core)
- **Frontend**: Vanilla CSS with modern styling and responsive design.
- **Testing**: Reqnroll (Gherkin/BDD) and Microsoft Playwright for end-to-end automation.
- **Environment**: Visual Studio Code / .NET 9 SDK

---

## Development Timeline
Detailed daily logs can be found in [DOCUMENTATION.md](./DOCUMENTATION.md).

### Week 4 (Jan 19 - Jan 23)
- **Planning**: Research on User Stories and Gherkin. Repository and communication setup.
- **Requirements**: Completion of User Story and Gherkin definitions.
- **Automation Start**: Initial Gherkin-based automated tests generated.

### Week 5 (Jan 26 - Jan 30)
- **Implementation**: UI development for Admin/Customer and SQLite integration.
- **Optimization**: Search functionality, admin confirmation dialogs, and UI polish.
- **Advanced Features**: Batch selection mode for time slots.
- **Final Testing**: All 18 automated test cases passing, including concurrency tests.

---

## Testing Strategy
HungryCalendar follows a **Behavior-Driven Development (BDD)** approach.

- **Features**: Defined in `.feature` files using Gherkin syntax (Given/When/Then).
- **Automation**: Steps are mapped to C# using Playwright to simulate real browser interactions.
- **Coverage**: Includes UI validation, successful booking flows, error handling, and admin tools.
- **Concurrency**: Tests simulate race conditions to ensure "First-Come-First-Serve" logic.

---

## Getting Started

### Prerequisites
- .NET 9 SDK
- PowerShell/Terminal

### Running the Project
```powershell
dotnet run --project HungryCalendar.Web/HungryCalendar.Web.csproj
```

### Running Tests
1. Install Playwright browsers:
   ```powershell
   cd HungryCalendar.Tests
   pwsh bin/Debug/net9.0/playwright.ps1 install
   ```
2. Execute tests:
   ```powershell
   dotnet test
   ```

---

*Project by the HungryCalendar Team (2026).*
