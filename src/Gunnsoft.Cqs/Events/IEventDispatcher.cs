using System.Threading;
using System.Threading.Tasks;

namespace Gunnsoft.Cqs.Events
{
    public interface IEventDispatcher
    {
        Task DispatchAsync(IEvent @event, CancellationToken cancellationToken);
    }
}