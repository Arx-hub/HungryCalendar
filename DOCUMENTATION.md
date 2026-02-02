# Development Timeline

This document tracks the daily progress and key milestones of the HungryCalendar project.

## Week 4: Planning and Foundation

### Monday 19.01
- Researched User Stories and Gherkin syntax.
- Initialized GitHub repository.
- Established communication channels (WhatsApp, Teams).
- Drafted initial User Stories and features.

### Tuesday 20.01
- Completed Moodle assignments 1 & 2: Defined formal User Stories and Gherkin scenarios.

### Wednesday 21.01
- Conducted project review with instructors.
- Refined User Stories and Gherkin definitions based on feedback.
- Updated documentation in Moodle.

### Thursday 22.01
- Explored Gherkin automation with AI assistance.
- Generated initial automated test scripts based on defined scenarios.
- Integrated automated tests into the GitHub repository.

### Friday 23.01
- Focused on automation tests.
- Identified need for a "UI-first" approach; decided to restart with UI implementation before finalizing tests.

## Week 5: Implementation and Refinement

### Monday 26.01
- Team members absent; project continued on new branch `ui_1`.
- Developed functional UI for both Administrator and Customer views.
- Implemented SQLite database for persistent reservation storage.

### Tuesday 27.01
- Reviewed and refined all test cases; added new scenarios.
- Achieved "Green" status for initial 12 tests.
- Added admin confirmation dialogs and improved time slot recovery logic.

### Wednesday 28.01
- Enhanced UI aesthetics and added search functionality.
- Expanded admin view to include customer contact details (phone numbers).
- Added scenarios for searching reservations and blocking entire days or multiple slots.

### Thursday 29.01
- Implemented advanced admin tools: Batch blocking/unblocking and selection cancellation.
- Switched development environment to VS Code agent.
- Resolved issues with reservation deletion logic in the admin panel.

### Friday 30.01
- Finalized code logic and Gherkin phrasing (e.g., "First Come First Serve").
- Implemented thread-safe booking using `using` statements for database transactions.
- **Milestone**: Successfully passed all 18 automated test cases.

---
*Last Updated: February 2, 2026*
