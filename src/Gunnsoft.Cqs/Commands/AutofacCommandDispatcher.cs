using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Gunnsoft.Cqs.Events;
using Microsoft.Extensions.Logging;

namespace Gunnsoft.Cqs.Commands
{
    public class AutofacCommandDispatcher : ICommandDispatcher
    {
        private static readonly Type s_commandHandlerGenericType;
        private readonly IComponentContext _componentContext;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ILogger<AutofacCommandDispatcher> _logger;

        static AutofacCommandDispatcher()
        {
            s_commandHandlerGenericType = typeof(ICommandHandler<,>);
        }

        public AutofacCommandDispatcher(IComponentContext componentContext, IEventDispatcher eventDispatcher,
            ILogger<AutofacCommandDispatcher> logger)
        {
            _componentContext = componentContext;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        public async Task<TEvent> DispatchAsync<TEvent>(ICommand<TEvent> command, CancellationToken cancellationToken)
            where TEvent : IEvent
        {
            var commandHandlerType = s_commandHandlerGenericType.MakeGenericType(command.GetType(), typeof(TEvent));
            var handleMethod = commandHandlerType.GetTypeInfo().GetMethod("HandleAsync");
            var commandHandler = _componentContext.Resolve(commandHandlerType);

            TEvent @event;

            @event = await (dynamic) handleMethod.Invoke(commandHandler, new object[] {command, cancellationToken});

            try
            {
                await _eventDispatcher.DispatchAsync(@event, cancellationToken);
            }
            catch (Exception exception)
            {
                var eventName = @event.GetType().FullName;
                var exceptionName = exception.GetType().FullName;

                _logger.LogError
                (
                    new EventId(),
                    exception,
                    "Exception {ExceptionName} thrown with message {ExceptionMessage} when handling event {EventName}",
                    exceptionName,
                    exception.Message,
                    eventName
                );
            }

            return @event;
        }
    }
}