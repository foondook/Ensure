# Login Feature

## Successful Login
- When I navigate to "/login"
- And I enter "test@example.com" into "email"
- And I enter "password123" into "password"
- And I click "Sign In"
- Then I should see text "Welcome back"

## Invalid Credentials
- When I navigate to "/login"
- And I enter "wrong@example.com" into "email"
- And I enter "wrongpass" into "password"
- And I click "Sign In"
- Then I should see text "Invalid credentials" 