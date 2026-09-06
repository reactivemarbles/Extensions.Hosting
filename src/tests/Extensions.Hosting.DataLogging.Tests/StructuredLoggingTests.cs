// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using log4net;
using log4net.Appender;
using log4net.Config;

namespace Extensions.Hosting.DataLogging.Tests;

/// <summary>Verifies that structured message fields reach Log4Net appenders.</summary>
public class StructuredLoggingTests
{
    /// <summary>Verifies message templates preserve typed fields and their original format.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task MessageTemplate_PreservesTypedFieldsAndOriginalFormat()
    {
        var repositoryName = $"structured-{Guid.NewGuid():N}";
        var repository = LogManager.CreateRepository(repositoryName);
        var appender = new MemoryAppender();
        _ = BasicConfigurator.Configure(repository, appender);

        try
        {
            var scopes = new LoggerExternalScopeProvider();
            var logger = new Log4NetLogger(
                new Log4NetProviderOptions
                {
                    Name = nameof(StructuredLoggingTests),
                    LoggerRepository = repositoryName,
                    LogLevelTranslator = new Log4NetLogLevelTranslator(),
                    LoggingEventFactory = new Log4NetLoggingEventFactory(),
                },
                scopes);
            using var scope = scopes.Push(new Dictionary<string, object?> { ["OrderId"] = "ambient", ["Tenant"] = "north" });
            const int orderId = 42;
            const decimal amount = 12.5M;
            const string template = "Order {OrderId} costs {Amount}";
            var logOrder = LoggerMessage.Define<int, decimal>(LogLevel.Information, default, template);
            logOrder(logger, orderId, amount, null);

            var events = appender.GetEvents();
            await Assert.That(events.Length).IsEqualTo(1);
            var loggingEvent = events[0];
            await Assert.That(loggingEvent.Properties["OrderId"]).IsEqualTo(orderId);
            await Assert.That(loggingEvent.Properties["Amount"]).IsEqualTo(amount);
            await Assert.That(loggingEvent.Properties["Tenant"]).IsEqualTo("north");
            await Assert.That(loggingEvent.Properties["{OriginalFormat}"]).IsEqualTo(template);
            await Assert.That(loggingEvent.RenderedMessage).IsEqualTo("Order 42 costs 12.5");
        }
        finally
        {
            repository.Shutdown();
        }
    }

    /// <summary>Verifies custom structured state retains nulls and objects while protecting the event identifier.</summary>
    /// <returns>A task representing the test.</returns>
    [Test]
    public async Task CustomState_PreservesValuesAndProtectsEventId()
    {
        var value = new object();
        const string emptyKey = "empty";
        const string duplicateKey = "duplicate";
        const int eventIdentifier = 7;
        var state = new List<KeyValuePair<string, object?>> { new("value", value), new(emptyKey, null), new("eventId", "state override"), new(duplicateKey, "first"), new(duplicateKey, "last") };
        var eventId = new EventId(eventIdentifier, "created");
        var candidate = new MessageCandidate<List<KeyValuePair<string, object?>>>(
            LogLevel.Information,
            eventId,
            state,
            null,
            static (_, _) => "formatted");
        var factory = new Log4NetLoggingEventFactory();
        var options = new Log4NetProviderOptions { LogLevelTranslator = new Log4NetLogLevelTranslator() };
        var loggingEvent = factory.CreateLoggingEvent(
            in candidate,
            LogManager.GetLogger(nameof(StructuredLoggingTests)).Logger,
            options,
            new LoggerExternalScopeProvider());

        await Assert.That(loggingEvent).IsNotNull();
        await Assert.That(loggingEvent!.Properties["value"]).IsSameReferenceAs(value);
        await Assert.That(loggingEvent.Properties.Contains(emptyKey)).IsTrue();
        await Assert.That(loggingEvent.Properties[emptyKey]).IsNull();
        await Assert.That(loggingEvent.Properties["eventId"]).IsEqualTo(eventId);
        await Assert.That(loggingEvent.Properties[duplicateKey]).IsEqualTo("last");
        await Assert.That(loggingEvent.RenderedMessage).IsEqualTo("formatted");
    }
}
