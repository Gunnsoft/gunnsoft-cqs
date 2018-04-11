using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace Gunnsoft.Cqs.Events
{
    public class RetryEventHandlerDecorator<TEvent> : IEventHandler<TEvent>
        where TEvent : IEvent
    {
        private readonly IEventHandler<TEvent> _decorated;
        private readonly ILogger<RetryEventHandlerDecorator<TEvent>> _logger;
        private readonly CloudStorageAccount _storageAccount;

        public RetryEventHandlerDecorator(IEventHandler<TEvent> decorated,
            ILogger<RetryEventHandlerDecorator<TEvent>> logger, CloudStorageAccount storageAccount)
        {
            _decorated = decorated;
            _logger = logger;
            _storageAccount = storageAccount;
        }

        public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken)
        {
            const int retryCount = 3;
            const int retryIntervalInMilliseconds = 100;

            var eventName = @event.GetType().FullName;
            var exceptions = new List<Exception>();

            for (var i = 0; i < retryCount; i++)
            {
                try
                {
                    Thread.Sleep(i * retryIntervalInMilliseconds);

                    await _decorated.HandleAsync(@event, cancellationToken);

                    return;
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            try
            {
                var queueClient = _storageAccount.CreateCloudQueueClient();
                var queue = queueClient.GetQueueReference($"{eventName.ToLower()}-poison");
                await queue.CreateIfNotExistsAsync();
                var message = new CloudQueueMessage(JsonConvert.SerializeObject(@event));
                await queue.AddMessageAsync(message);
            }
            catch (Exception exception)
            {
                var exceptionName = exception.GetType().FullName;

                _logger.LogError
                (
                    exception,
                    "Exception {ExceptionName} thrown with message {ExceptionMessage} when storing poisoned event {EventName}",
                    exceptionName,
                    exception.Message,
                    eventName
                );
            }

            throw new AggregateException(exceptions);
        }
    }
}