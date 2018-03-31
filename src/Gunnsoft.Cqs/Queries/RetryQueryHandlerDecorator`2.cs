using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gunnsoft.Cqs.Queries
{
    public class RetryQueryHandlerDecorator<TQuery, TProjection> : IQueryHandler<TQuery, TProjection>
        where TQuery : IQuery<TProjection>
        where TProjection : IProjection
    {
        private readonly IQueryHandler<TQuery, TProjection> _decorated;

        public RetryQueryHandlerDecorator(IQueryHandler<TQuery, TProjection> decorated)
        {
            _decorated = decorated;
        }

        public async Task<TProjection> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            const int retryCount = 3;
            const int retryIntervalInMilliseconds = 100;

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