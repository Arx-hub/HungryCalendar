Feature: Admin Management
  As an admin
  I want to be able to manage reservations and settings
  So that the restaurant runs smoothly

  Scenario: Administrator defines reservation times in 15-minute intervals
    Given the administrator is logged into the reservation system
    When the administrator defines available reservation times
    Then only time slots in 15-minute intervals are selectable
    And other time values are rejected

  Scenario: Administrator removes available times
    Given the administrator is logged into the reservation system
    When the administrator disables a specific time slot
    Then the time slot becomes unavailable to customers

  Scenario: Administrator views group size for a reservation
    Given the administrator is viewing a reservation in the calendar
    When the reservation details are opened
    Then the group size is displayed
    And the group size is clearly visible and correct

  Scenario: Administrator needs to remove a reservation because a customer canceled it
    Given the administrator is logged into the reservation system
    When the administrator selects a reservation and deletes it
    Then the reservation is removed from the calendar
    And the time slot becomes available again

  Scenario: Administrator re-enables a disabled time slot
    Given the administrator is logged into the reservation system
    And the administrator has disabled a specific time slot
    When the administrator clicks on the disabled time slot
    Then the time slot becomes available again

  Scenario: Administrator searches for a reservation
    Given the administrator is logged into the reservation system
    And there are reservations for "John unique_search_test", "Jane Smith", and "Bob Wilson"
    When the administrator searches for "unique_search_test"
    Then only the reservation for "John unique_search_test" is displayed
    And the reservations for "Jane Smith" and "Bob Wilson" are not displayed
