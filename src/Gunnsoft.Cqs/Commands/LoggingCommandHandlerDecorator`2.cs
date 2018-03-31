using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Gunnsoft.Cqs.Events;
using Microsoft.Extensions.Logging;

namespace Gunnsoft.Cqs.Commands
{
    public class LoggingCommandHandlerDecorator<TCommand, TEvent> : ICommandHandler<TCommand, TEvent>
        where TCommand : ICommand<TEvent>
        where TEvent : IEvent
    {
        private readonly ICommandHandler<TCommand, TEvent> _decorated;
        private readonly ILogger<LoggingCommandHandlerDecorator<TCommand, TEvent>> _logger;

        public LoggingCommandHandlerDecorator(ICommandHandler<TCommand, TEvent> decorated,
            ILogger<LoggingCommandHandlerDecorator<TCommand, TEvent>> logger)
        {
            _decorated = decorated;
            _logger = logger;
        }

        public async Task<TEvent> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            var commandName = command.GetType().FullName;
            var commandHandlerName = _decorated.GetType().FullName;

            var stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();

                return await _decorated.HandleAsync(command, cancellationToken);
            }
            catch (Exception exception)
            {
                var exceptionName = exception.GetType().FullName;

                _logger.LogWarning
                (
                    exception,
                    "Exception {ExceptionName} thrown with message {ExceptionMessage} when handling command {CommandName} using handler {CommandHandlerName}",
                    exceptionName,
                    exception.Message,
                    commandName,
                    commandHandlerName
                );

                throw;
            }
            finally
            {
                stopwatch.Stop();

                var elapsed = stopwatch.Elapsed.TotalMilliseconds;

                _logger.LogInformation
                (
                    "Handled command {CommandName} using handler {CommandHandlerName} in {ElapsedMilliseconds}ms",
                    commandName,
                    commandHandlerName,
                    elapsed
                );

                _logger.LogDebug
                (
                    "{@Command}",
                    command
                );
            }
        }
    }
}