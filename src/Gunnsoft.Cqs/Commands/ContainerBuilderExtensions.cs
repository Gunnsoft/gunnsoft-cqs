using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;

namespace Gunnsoft.Cqs.Commands
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder AddCommandDispatcher(this ContainerBuilder extended)
        {
            extended.RegisterType<AutofacCommandDispatcher>()
                .As<ICommandDispatcher>()
                .InstancePerDependency();

            return extended;
        }

        public static ContainerBuilder AddCommandHandlerDecorators(this ContainerBuilder extended)
        {
            extended.RegisterGenericDecorator
                (
                    typeof(LoggingCommandHandlerDecorator<,>),
                    typeof(ICommandHandler<,>),
                    "CommandHandler",
                    "LoggingCommandHandler"
                )
                .InstancePerDependency();

            extended.RegisterGenericDecorator
                (
                    typeof(RetryCommandHandlerDecorator<,>),
                    typeof(ICommandHandler<,>),
                    "LoggingCommandHandler"
                )
                .InstancePerDependency();

            return extended;
        }

        public static ContainerBuilder AddCommandHandlers(this ContainerBuilder extended)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                AddCommandHandlers(assembly);
            }

            void AddCommandHandlers(Assembly assembly)
            {
                extended.RegisterAssemblyTypes(assembly)
                    .As(t => t.GetInterfaces()
                        .Where(i => i.IsClosedTypeOf(typeof(ICommandHandler<,>)))
                        .Select(i => new KeyedService("CommandHandler", i)))
                    .InstancePerDependency();
            }

            return extended;
        }
    }
}