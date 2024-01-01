using Xunit;
using FluentAssertions;
using System;

namespace NLOverlay.Tests
{
    public class SettingsTests : IDisposable
    {
        public SettingsTests()
        {
            // Initialization logic if needed
        }

        [Fact]
        public void TT()
        {
            var test = "hi";
            test.Should().Be("hi");
        }

        public void Dispose()
        {
            // Cleanup logic
        }
    }
}
