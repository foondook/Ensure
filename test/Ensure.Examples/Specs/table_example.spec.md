# User Data Validation

## Validate Multiple Users

* When I validate multiple users data

| Name  | Age | Email           |
|-------|-----|-----------------|
| John  | 25  | john@email.com  |
| Alice | 30  | alice@email.com |

* Then all users should be valid

## Invalid User Data

* When I validate multiple users data

| Name  | Age | Email      |
|-------|-----|------------|
| Bob   | -1  | not_email  |

* Then validation should fail with "Invalid age and email format" 