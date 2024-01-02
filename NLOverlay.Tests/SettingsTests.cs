using FluentAssertions;
using NLOverlay.Models;

namespace NLOverlay.Tests
{
    public class SettingsTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(61000)]
        public void OutOfBoundsPollingRate_Should_Not_Pass_Validation(int pollingRate)
        {
            // Arrange
            var settings = new Settings();

            // Act
            settings.ApiPollingRate = pollingRate.ToString();
            var validationResult = settings.Validate();

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Count().Should().BeGreaterThanOrEqualTo(0);
            validationResult.Errors.Should().Contain("API Polling Rate must be between 0 and 60000 ms.");
        }
    }
}