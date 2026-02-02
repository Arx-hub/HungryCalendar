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
    Given the administrator is viewing the reservations in the admin view
    When the reservations are visible
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

  Scenario: Administrator blocks entire day to prevent all reservations
    Given the administrator is logged into the reservation system
    And the administrator navigates to a future date
    When the administrator clicks the "Block all times" checkbox
    Then all time slots for that day become unavailable to customers
    And the "Block all times" checkbox remains checked
    
  Scenario: Administrator unblocks an entire day to allow reservations again
    Given the administrator is logged into the reservation system
    And the administrator navigates to a future date
    And the administrator has blocked the entire day by checking "Block all times"
    When the administrator unchecks the "Block all times" checkbox
    Then all time slots for that day become available again
    And the "Block all times" checkbox is no longer checked

  Scenario: Administrator blocks selected time slots
    Given the administrator is logged into the reservation system
    And the administrator navigates to a future date
    When the administrator clicks the "Select Multiple" button
    Then each time slot displays a checkbox for selection
    And the "Block Selected Times", "Unblock Selected Times", and "Cancel Selection" buttons are visible
    When the administrator selects at least one time slot
    Then the selected time slots are highlighted
    And the checkboxes for selected time slots are checked
    When the administrator clicks the "Block Selected Times" button
    Then the selected time slots become blocked
    And the blocked time slots are unavailable for customers to select
    And the selection mode is exited
    And the batch action buttons are no longer visible

  Scenario: Administrator unblocks selected time slots
    Given the administrator is logged into the reservation system
    And there are some blocked time slots
    When the administrator clicks the "Select Multiple" button
    Then each time slot displays a checkbox for selection
    And the "Block Selected Times", "Unblock Selected Times", and "Cancel Selection" buttons are visible
    When the administrator selects at least one blocked time slot
    Then the selected time slots are highlighted
    And the checkboxes for selected time slots are checked
    When the administrator clicks the "Unblock Selected Times" button
    Then the selected time slots become unblocked
    And the unblocked time slots are available for customers to select
    And the selection mode is exited
    And the batch action buttons are no longer visible

  Scenario: Administrator cancels time slot selection
    Given the administrator is logged into the reservation system
    When the administrator clicks the "Select Multiple" button
    Then each time slot displays a checkbox for selection
    And the "Block Selected Times", "Unblock Selected Times", and "Cancel Selection" buttons are visible
    When the administrator selects at least one time slot
    Then the selected time slots are highlighted
    And the checkboxes for selected time slots are checked
    When the administrator clicks the "Cancel Selection" button
    Then the selection mode is exited
    And no time slots are selected
    And the batch action buttons are no longer visible