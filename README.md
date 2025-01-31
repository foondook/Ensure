# Ensure.Generator

A .NET tool for generating test code from specification files.

[![NuGet](https://img.shields.io/nuget/v/Ensure.Generator.svg)](https://www.nuget.org/packages/Ensure.Generator)
[![NuGet](https://img.shields.io/nuget/dt/Ensure.Generator.svg)](https://www.nuget.org/packages/Ensure.Generator)
[![License](https://img.shields.io/github/license/foondook/ensure)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://www.nuget.org/packages/Ensure.Generator)

Inspired by [SpecFlow](https://specflow.org/) and [Gauge](https://gauge.org/), but with a focus on simplicity and modern features. Ensure.Generator takes a lightweight approach by focusing solely on generating clean, typed test code from simple markdown specifications.

## Why Executable Specifications?

In today's rapidly evolving software landscape, acceptance tests and executable specifications are becoming increasingly crucial. They serve as living documentation that evolves with your codebase, ensuring that your tests always reflect the current business requirements. Learn more about why this approach is gaining traction in this [detailed overview video](https://youtu.be/NsOUKfzyZiU).

## Installation

```bash
dotnet tool install --global Ensure.Generator
```

## Usage

1. Create a specification file (e.g., `login.spec.md`) in your test project
2. Run the generator:

```bash
# For C#
ensure csharp -s path/to/specs -o path/to/output -n YourNamespace

# For TypeScript
ensure typescript -s path/to/specs -o path/to/output
```

## Examples

### Login Feature Specification

```markdown
# Login Feature

## Successful Login
- Navigate to "/login"
- Enter "test@example.com" into "email" field
- Enter "password123" into "password" field
- Click "Sign In" button
- Verify text "Welcome back" is shown

## Invalid Credentials
- Navigate to "/login"
- Enter "wrong@example.com" into "email" field
- Enter "wrongpass" into "password" field
- Click "Sign In" button
- Verify text "Invalid credentials" is shown
```

### Table-Driven Tests

```markdown
# User Data Validation

## Validate Multiple Users
- Load test users
| Name  | Age | Email           |
|-------|-----|-----------------|
| John  | 25  | john@email.com  |
| Alice | 30  | alice@email.com |
- Validate all users
- Check validation results
```

## Generated Code

The tool generates both test classes and step definitions in C# or TypeScript.

### C# Generated Code

```csharp
// Generated Steps Base Class
public abstract class LoginFeatureStepsBase
{
    /// <summary>
    /// Navigate to "/login"
    /// </summary>
    public abstract Task NavigateTo(string param1);

    /// <summary>
    /// Enter "test@example.com" into "email" field
    /// </summary>
    public abstract Task EnterIntoField(string param1, string param2);

    /// <summary>
    /// Click "Sign In" button
    /// </summary>
    public abstract Task ClickButton(string param1);

    /// <summary>
    /// Verify text "Welcome back" is shown
    /// </summary>
    public abstract Task VerifyTextIsShown(string param1);
}

// Generated Tests Base Class
public abstract class LoginFeatureTestsBase
{
    protected abstract LoginFeatureStepsBase Steps { get; }

    [Fact]
    public async Task SuccessfulLogin()
    {
        await Steps.NavigateTo("/login");
        await Steps.EnterIntoField("test@example.com", "email");
        await Steps.EnterIntoField("password123", "password");
        await Steps.ClickButton("Sign In");
        await Steps.VerifyTextIsShown("Welcome back");
    }

    // ... other test methods
}
```

### TypeScript Generated Code

```typescript
// Generated Steps Base Class
export abstract class LoginFeatureBase {
    /**
     * Navigate to "/login"
     */
    abstract navigateTo(param1: string): Promise<void>;

    /**
     * Enter "test@example.com" into "email" field
     */
    abstract enterIntoField(param1: string, param2: string): Promise<void>;

    /**
     * Click "Sign In" button
     */
    abstract clickButton(param1: string): Promise<void>;

    /**
     * Verify text "Welcome back" is shown
     */
    abstract verifyTextIsShown(param1: string): Promise<void>;

    // ... other step definitions
}

// Generated Tests Base Class
export abstract class LoginFeatureTestsBase {
    protected abstract getSteps(): LoginFeatureBase;

    async successfulLogin() {
        const steps = this.getSteps();

        await steps.navigateTo('/login');
        await steps.enterIntoField('test@example.com', 'email');
        await steps.enterIntoField('password123', 'password');
        await steps.clickButton('Sign In');
        await steps.verifyTextIsShown('Welcome back');
    }

    // ... other test methods
}
```

To implement the tests, create concrete classes that inherit from the generated base classes:

### C# Implementation

```csharp
public class LoginFeatureTests : LoginFeatureTestsBase
{
    protected override LoginFeatureSteps Steps => new();
}

public class LoginFeatureSteps : LoginFeatureStepsBase
{
    public override async Task NavigateTo(string url)
    {
        // Your implementation here
    }

    public override async Task EnterIntoField(string text, string field)
    {
        // Your implementation here
    }

    public override async Task ClickButton(string button)
    {
        // Your implementation here
    }

    public override async Task VerifyTextIsShown(string text)
    {
        // Your implementation here
    }
}
```

### TypeScript Implementation

```typescript
class LoginFeatureSteps extends LoginFeatureBase {
    async navigateTo(url: string): Promise<void> {
        // Your implementation here
    }

    async enterIntoField(text: string, field: string): Promise<void> {
        // Your implementation here
    }

    async clickButton(button: string): Promise<void> {
        // Your implementation here
    }

    async verifyTextIsShown(text: string): Promise<void> {
        // Your implementation here
    }
}

class LoginFeatureTests extends LoginFeatureTestsBase {
    protected getSteps(): LoginFeatureBase {
        return new LoginFeatureSteps();
    }
}
```

## Features

- Generates test code from markdown specification files
- Supports both C# and TypeScript output
- Clean, typed step definitions
- Simple bullet-point style steps
- Table-driven test scenarios
- Automatic parameter extraction from quoted strings
- No Gherkin/Cucumber syntax - just plain English

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.