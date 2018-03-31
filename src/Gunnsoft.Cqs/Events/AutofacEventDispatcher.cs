using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;

namespace Gunnsoft.Cqs.Events
{
    public class AutofacEventDispatcher : IEventDispatcher
    {
        private static readonly Type s_eventHandlerGenericType;
        private static readonly Type s_eventHandlersGenericType;
        private readonly IComponentContext _componentContext;

        static AutofacEventDispatcher()
        {
            s_eventHandlersGenericType = typeof(IReadOnlyCollection<>);
            s_eventHandlerGenericType = typeof(IEventHandler<>);
        }

        public AutofacEventDispatcher(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        public async Task DispatchAsync(IEvent @event, CancellationToken cancellationToken)
        {
            var eventHandlerType = s_eventHandlerGenericType.MakeGenericType(@event.GetType());
            var handleMethod = eventHandlerType.GetTypeInfo().GetMethod("HandleAsync");
            var collectionType = s_eventHandlersGenericType.MakeGenericType(eventHandlerType);
            var eventHandlers = _componentContext.Resolve(collectionType) as dynamic;

            if (eventHandlers == null)
            {
                return;
            }

            var exceptions = new List<Exception>();

            foreach (var eventHandler in eventHandlers)
            {
                await handleMethod.Invoke(eventHandler, new object[] {@event, cancellationToken});
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}