using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Gunnsoft.Cqs.Events
{
    public class LoggingEventHandlerDecorator<TEvent> : IEventHandler<TEvent>
        where TEvent : IEvent
    {
        private readonly IEventHandler<TEvent> _decorated;
        private readonly ILogger<LoggingEventHandlerDecorator<TEvent>> _logger;

        public LoggingEventHandlerDecorator(IEventHandler<TEvent> decorated,
            ILogger<LoggingEventHandlerDecorator<TEvent>> logger)
        {
            _decorated = decorated;
            _logger = logger;
        }

        public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken)
        {
            var eventName = @event.GetType().FullName;
            var eventHandlerName = _decorated.GetType().FullName;

            var stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();

                await _decorated.HandleAsync(@event, cancellationToken);
            }
            catch (Exception exception)
            {
                var exceptionName = exception.GetType().FullName;

                _logger.LogWarning
                (
                    exception,
                    "Exception {ExceptionName} thrown with message {ExceptionMessage} when handling event {EventName} with handler {EventHandlerName}",
                    exceptionName,
                    exception.Message,
                    eventName,
                    eventHandlerName
                );

                throw;
            }
            finally
            {
                stopwatch.Stop();

                var elapsed = stopwatch.Elapsed.TotalMilliseconds;

                _logger.LogInformation
                (
                    "Handled event {EventName} using handler {EventHandlerName} in {ElapsedMilliseconds}ms",
                    eventName,
                    eventHandlerName,
                    elapsed
                );

                _logger.LogDebug
                (
                    "{@Event}",
                    @event
                );
            }
        }
    }
}