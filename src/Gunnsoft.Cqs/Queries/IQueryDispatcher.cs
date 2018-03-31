using System.Threading;
using System.Threading.Tasks;

namespace Gunnsoft.Cqs.Queries
{
    public interface IQueryDispatcher
    {
        Task<TProjection> DispatchAsync<TProjection>(IQuery<TProjection> query, CancellationToken cancellationToken)
            where TProjection : IProjection;
    }
}