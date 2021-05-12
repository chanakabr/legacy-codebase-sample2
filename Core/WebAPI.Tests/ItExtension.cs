using FluentAssertions;
using Moq;

namespace WebAPI.Tests
{
    // not used for now, but leaving it as an example of deep-equal assertion
    public static class ItExtension
    {
        public static T IsDeepEqual<T>(T expected)
        {
            return Match.Create<T>(actual => Validate(actual, expected));
        }
        
        private static bool Validate<T>(T actual, T expected)
        {
            actual.Should().BeEquivalentTo(expected);
            return true;
        }
    }
}
