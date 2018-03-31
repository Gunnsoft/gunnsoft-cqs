using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Gunnsoft.Cqs.Queries
{
    public class LoggingQueryHandlerDecorator<TQuery, TProjection> : IQueryHandler<TQuery, TProjection>
        where TQuery : IQuery<TProjection>
        where TProjection : IProjection
    {
        private readonly IQueryHandler<TQuery, TProjection> _decorated;
        private readonly ILogger<LoggingQueryHandlerDecorator<TQuery, TProjection>> _logger;

        public LoggingQueryHandlerDecorator(IQueryHandler<TQuery, TProjection> decorated,
            ILogger<LoggingQueryHandlerDecorator<TQuery, TProjection>> logger)
        {
            _decorated = decorated;
            _logger = logger;
        }

        public async Task<TProjection> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            var queryName = query.GetType().FullName;
            var queryHandlerName = _decorated.GetType().FullName;

            var stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();

                return await _decorated.HandleAsync(query, cancellationToken);
            }
            catch (Exception exception)
            {
                var exceptionName = exception.GetType().FullName;

                _logger.LogWarning
                (
                    exception,
                    "Exception {ExceptionName} thrown with message {ExceptionMessage} when handling query {QueryName} using handler {QueryHandlerName}",
                    exceptionName,
                    exception.Message,
                    queryName,
                    queryHandlerName
                );

                throw;
            }
            finally
            {
                stopwatch.Stop();

                var elapsed = stopwatch.Elapsed.TotalMilliseconds;

                _logger.LogInformation
                (
                    "Handled query {QueryName} using handler {QueryHandlerName} in {ElapsedMilliseconds}ms",
                    queryName,
                    queryHandlerName,
                    elapsed
                );

                _logger.LogDebug
                (
                    "{@Query}",
                    query
                );
            }
        }
    }
}