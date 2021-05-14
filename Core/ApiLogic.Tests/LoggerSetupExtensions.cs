using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiLogic.Tests
{
    internal static class LoggerSetupExtensions
    {
        public static Mock<ILogger> Setup(this Mock<ILogger> logger, LogLevel logLevel, string expectedMessage)
        {
            Func<object, Type, bool> state = (v, t) => string.Compare(v.ToString(), expectedMessage, StringComparison.Ordinal) == 0;

            logger.Setup(x => x.Log(
                    logLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => state(v, t)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            return logger;
        }
    }
}
