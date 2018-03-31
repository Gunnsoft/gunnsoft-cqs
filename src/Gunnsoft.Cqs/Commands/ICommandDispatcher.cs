using System.Threading;
using System.Threading.Tasks;
using Gunnsoft.Cqs.Events;

namespace Gunnsoft.Cqs.Commands
{
    public interface ICommandDispatcher
    {
        Task<TEvent> DispatchAsync<TEvent>(ICommand<TEvent> command, CancellationToken cancellationToken)
            where TEvent : IEvent;
    }
}