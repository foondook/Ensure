// <auto-generated />
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Ensure.Examples.DataValidation
{
    public abstract class UserDataValidationTestsBase
    {
        protected abstract UserDataValidationStepsBase Steps { get; }


        [Fact]
        public async Task ValidateMultipleUsers()
        {
            await Steps.WhenIValidateMultipleUsersData(new()
            {
                new() { ["Name"] = "John", ["Age"] = "25", ["Email"] = "john@email.com" },
                new() { ["Name"] = "Alice", ["Age"] = "30", ["Email"] = "alice@email.com" },
            });
            await Steps.ThenAllUsersShouldBeValid();
        }

        [Fact]
        public async Task InvalidUserData()
        {
            await Steps.WhenIValidateMultipleUsersData(new()
            {
                new() { ["Name"] = "Bob", ["Age"] = "-1", ["Email"] = "not_email" },
            });
            await Steps.ThenValidationShouldFailWith("Invalid age and email format");
        }
    }
}
