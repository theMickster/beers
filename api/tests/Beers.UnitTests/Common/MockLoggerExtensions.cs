﻿using Microsoft.Extensions.Logging;

namespace Beers.UnitTests.Common;

internal static class MockLoggerExtensions
{
    /// <summary>
    /// Handles the verification of log messages being passed to a Moq ILogger object.
    /// Verifies the message logged is exactly <paramref name="expectedMessage"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="logger"></param>
    /// <param name="expectedMessage"></param>
    /// <param name="expectedEvent"></param>
    /// <param name="expectedLogLevel"></param>
    /// <param name="times"></param>
    /// <returns></returns>
    internal static Mock<ILogger<T>> VerifyLoggingMessageIs<T>(this Mock<ILogger<T>> logger, string expectedMessage, int? expectedEvent = null, LogLevel expectedLogLevel = LogLevel.Debug, Times? times = null)
    {
        times ??= Times.Once();

        Func<object, Type, bool> state = (v, t) => string.Compare(v.ToString()!, expectedMessage, StringComparison.InvariantCultureIgnoreCase) == 0;

        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == expectedLogLevel),
                expectedEvent == null ? It.IsAny<EventId>() : new EventId(Convert.ToInt32(expectedEvent), string.Empty),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), (Times)times);

        return logger;
    }

    /// <summary>
    /// Handles the verification of log messages being passed to a Moq ILogger object.
    /// Verifies the message logged ends with <paramref name="expectedMessage"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="logger"></param>
    /// <param name="expectedMessage"></param>
    /// <param name="expectedEvent"></param>
    /// <param name="expectedLogLevel"></param>
    /// <param name="times"></param>
    /// <returns></returns>
    internal static Mock<ILogger<T>> VerifyLoggingMessageEndsWith<T>(this Mock<ILogger<T>> logger, string expectedMessage, int? expectedEvent = null, LogLevel expectedLogLevel = LogLevel.Debug, Times? times = null)
    {
        times ??= Times.Once();

        Func<object, Type, bool> state = (v, t) => v.ToString()!.EndsWith(expectedMessage);

        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == expectedLogLevel),
                expectedEvent == null ? It.IsAny<EventId>() : new EventId(Convert.ToInt32(expectedEvent), string.Empty),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), (Times)times);
        return logger;
    }

    /// <summary>
    /// Handles the verification of log messages being passed to a Moq ILogger object.
    /// Verifies the message logged starts with <paramref name="expectedMessage"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="logger"></param>
    /// <param name="expectedMessage"></param>
    /// <param name="expectedEvent"></param>
    /// <param name="expectedLogLevel"></param>
    /// <param name="times"></param>
    /// <returns></returns>
    internal static Mock<ILogger<T>> VerifyLoggingMessageStartsWith<T>(this Mock<ILogger<T>> logger, string expectedMessage, int? expectedEvent = null, LogLevel expectedLogLevel = LogLevel.Debug, Times? times = null)
    {
        times ??= Times.Once();

        Func<object, Type, bool> state = (v, t) => v.ToString()!.StartsWith(expectedMessage);

        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == expectedLogLevel),
                expectedEvent == null ? It.IsAny<EventId>() : new EventId(Convert.ToInt32(expectedEvent), string.Empty),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), (Times)times);
        return logger;
    }

    /// <summary>
    /// Handles the verification of log messages being passed to a Moq ILogger object.
    /// Verifies the message logged contains <paramref name="expectedMessage"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="logger"></param>
    /// <param name="expectedMessage"></param>
    /// <param name="expectedEvent"></param>
    /// <param name="expectedLogLevel"></param>
    /// <param name="times"></param>
    /// <returns></returns>
    internal static Mock<ILogger<T>> VerifyLoggingMessageContains<T>(this Mock<ILogger<T>> logger, string expectedMessage, int? expectedEvent = null, LogLevel expectedLogLevel = LogLevel.Debug, Times? times = null)
    {
        times ??= Times.Once();

        Func<object, Type, bool> state = (v, t) => v.ToString()!.Contains(expectedMessage);

        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == expectedLogLevel),
                expectedEvent == null ? It.IsAny<EventId>() : new EventId(Convert.ToInt32(expectedEvent), string.Empty),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), (Times)times);
        return logger;
    }
}