using System.Threading;
using System.Threading.Tasks;

namespace Gunnsoft.Cqs.Queries
{
    public interface IQueryHandler<in TQuery, TProjection>
        where TQuery : IQuery<TProjection>
        where TProjection : IProjection
    {
        Task<TProjection> HandleAsync(TQuery query, CancellationToken cancellationToken);
    }
}