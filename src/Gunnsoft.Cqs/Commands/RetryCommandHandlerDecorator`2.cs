using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gunnsoft.Cqs.Events;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace Gunnsoft.Cqs.Commands
{
    public class RetryCommandHandlerDecorator<TCommand, TEvent> : ICommandHandler<TCommand, TEvent>
        where TCommand : ICommand<TEvent>
        where TEvent : IEvent
    {
        private readonly ICommandHandler<TCommand, TEvent> _decorated;
        private readonly ILogger<RetryEventHandlerDecorator<TEvent>> _logger;
        private readonly CloudStorageAccount _storageAccount;

        public RetryCommandHandlerDecorator(ICommandHandler<TCommand, TEvent> decorated,
            ILogger<RetryEventHandlerDecorator<TEvent>> logger, CloudStorageAccount storageAccount)
        {
            _decorated = decorated;
            _logger = logger;
            _storageAccount = storageAccount;
        }

        public async Task<TEvent> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            const int retryCount = 3;
            const int retryIntervalInMilliseconds = 100;

            var commandName = command.GetType().Name;
            var commandFullName = command.GetType().FullName;
            var exceptions = new List<Exception>();

            for (var i = 0; i < retryCount; i++)
            {
                try
                {
                    Thread.Sleep(i * retryIntervalInMilliseconds);

                    return await _decorated.HandleAsync(command, cancellationToken);
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            try
            {
                var queueClient = _storageAccount.CreateCloudQueueClient();
                var queue = queueClient.GetQueueReference($"{commandName.ToLower()}-poison");
                await queue.CreateIfNotExistsAsync();
                var message = new CloudQueueMessage(JsonConvert.SerializeObject(command));
                await queue.AddMessageAsync(message);
            }
            catch (Exception exception)
            {
                var exceptionName = exception.GetType().FullName;

                _logger.LogError
                (
                    exception,
                    "Exception {ExceptionName} thrown with message {ExceptionMessage} when storing poisoned command {CommandName}",
                    exceptionName,
                    exception.Message,
                    commandFullName
                );
            }

            throw new AggregateException(exceptions);
        }
    }
}