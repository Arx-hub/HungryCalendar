Feature: Customer Booking
  As a customer
  I want to be able to reserve a certain sized table at a certain time
  So that I can have dinner at the restaurant

  Scenario: Customer makes a reservation
    Given the customer has selected an available time
    When the customer enters name, valid email, valid phone number and valid amount of people
    Then the system saves the reservation
    And displays a confirmation message "Reservation successful"

  Scenario: No available reservation times
    Given the customer has selected a date and group size
    When no reservation times are available
    Then the system displays a message stating "No available times"

  Scenario: Hiding reserved times
    Given the customer is viewing the reservation calendar
    When the calendar is displayed
    Then all reserved times are not selectable
    And reserved times are not visible or disabled

  Scenario: Double booking is prevented
    Given a customer has selected a reservation time
    When another customer confirms the same time first
    Then the system informs the customer that the time is no longer available

  Scenario: Confirmation page is displayed after reservation
    Given the customer is making a reservation
    When the reservation is successfully completed
    Then a confirmation page is displayed
    And the page shows the reservation details
    And the page shows the restaurant’s contact information
