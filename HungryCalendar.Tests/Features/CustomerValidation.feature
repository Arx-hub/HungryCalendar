Feature: Customer Validation
  As a customer
  I want the system to tell me when I've entered an invalid form of information
  So that I can correct it

  Scenario: Invalid email is entered
    Given the customer is filling up the contact information
    When the customer enters an invalid email address
    Then a message will ask to provide a valid email address
    And the field is marked as invalid

  Scenario: Customer enters an invalid phone number
    Given the customer has selected an available reservation time
    When the customer enters an invalid phone number
    Then the phone number field is marked as invalid
    And the system asks the customer to check the phone number
