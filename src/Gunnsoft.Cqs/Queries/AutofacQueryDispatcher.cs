using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;

namespace Gunnsoft.Cqs.Queries
{
    public class AutofacQueryDispatcher : IQueryDispatcher
    {
        private static readonly Type s_queryHandlerGenericType;

        private readonly IComponentContext _componentContext;

        static AutofacQueryDispatcher()
        {
            s_queryHandlerGenericType = typeof(IQueryHandler<,>);
        }

        public AutofacQueryDispatcher(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        public async Task<TProjection> DispatchAsync<TProjection>(IQuery<TProjection> query,
            CancellationToken cancellationToken)
            where TProjection : IProjection
        {
            var queryHandlerType = s_queryHandlerGenericType.MakeGenericType(query.GetType(), typeof(TProjection));
            var handleMethod = queryHandlerType.GetTypeInfo().GetMethod("HandleAsync");
            var queryHandler = _componentContext.Resolve(queryHandlerType);

            return await (dynamic) handleMethod.Invoke(queryHandler, new object[] {query, cancellationToken});
        }
    }
}