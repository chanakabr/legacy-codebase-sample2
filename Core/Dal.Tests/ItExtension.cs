using FluentAssertions;
using Moq;

namespace DAL.Tests
{
    // an example of deep-equal assertion
    public static class ItExtension
    {
        public static T IsDeepEqual<T>(T expected)
        {
            return Match.Create<T>(actual => Validate(actual, expected));
        }

        public static bool Validate<T>(T actual, T expected)
        {
            actual.Should().BeEquivalentTo(expected);
            return true;
        }
    }
}
