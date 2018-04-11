using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Gunnsoft.Cqs.Queries
{
    public class RetryQueryHandlerDecorator<TQuery, TProjection> : IQueryHandler<TQuery, TProjection>
        where TQuery : IQuery<TProjection>
        where TProjection : IProjection
    {
        private readonly IQueryHandler<TQuery, TProjection> _decorated;
        private readonly ILogger<RetryQueryHandlerDecorator<TQuery, TProjection>> _logger;

        public RetryQueryHandlerDecorator(IQueryHandler<TQuery, TProjection> decorated,
            ILogger<RetryQueryHandlerDecorator<TQuery, TProjection>> logger)
        {
            _decorated = decorated;
            _logger = logger;
        }

        public async Task<TProjection> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            const int retryCount = 3;
            const int retryIntervalInMilliseconds = 100;

            var queryName = query.GetType().FullName;
            var exceptions = new List<Exception>();

            for (var i = 0; i < retryCount; i++)
            {
                try
                {
                    Thread.Sleep(i * retryIntervalInMilliseconds);

                    return await _decorated.HandleAsync(query, cancellationToken);
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }


            throw new AggregateException(exceptions);
        }
    }
}