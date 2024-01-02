using FluentAssertions;

namespace NLOverlay.Tests
{
    public class SettingsTests
    {
        [Fact]
        public void Test1()
        {
            var setting = "hi";

            setting.Should().Be("hi");
        }
    }
}