# User Authentication

* Navigate to "login" page
* Clear browser cookies
* Initialize test database

## User can login successfully
* Enter username "john.doe@example.com"
* Enter password "securePass123"
* Click login button
* Verify user is redirected to "dashboard"
* Verify welcome message shows "Welcome, John Doe"

## Login fails with invalid credentials
* Enter username "john.doe@example.com"
* Enter password "wrongpass"
* Click login button
* Verify error message "Invalid credentials"
* Verify user remains on "login" page 