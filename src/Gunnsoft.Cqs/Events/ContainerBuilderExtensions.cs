using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;

namespace Gunnsoft.Cqs.Events
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder AddEventDispatcher(this ContainerBuilder extended)
        {
            extended.RegisterType<AutofacEventDispatcher>()
                .As<IEventDispatcher>()
                .InstancePerDependency();

            return extended;
        }

        public static ContainerBuilder AddEventHandlerDecorators(this ContainerBuilder extended)
        {
            extended.RegisterGenericDecorator
                (
                    typeof(LoggingEventHandlerDecorator<>),
                    typeof(IEventHandler<>),
                    "EventHandler",
                    "LoggingEventHandler"
                )
                .InstancePerDependency();

            extended.RegisterGenericDecorator
                (
                    typeof(RetryEventHandlerDecorator<>),
                    typeof(IEventHandler<>),
                    "LoggingEventHandler"
                )
                .InstancePerDependency();

            return extended;
        }

        public static ContainerBuilder AddEventHandlers(this ContainerBuilder extended)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                AddEventHandlers(assembly);
            }

            void AddEventHandlers(Assembly assembly)
            {
                extended.RegisterAssemblyTypes(assembly)
                    .As(t => t.GetInterfaces()
                        .Where(i => i.IsClosedTypeOf(typeof(IEventHandler<>)))
                        .Select(i => new KeyedService("EventHandler", i)))
                    .InstancePerDependency();
            }


            return extended;
        }
    }
}