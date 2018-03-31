using System.Threading;
using System.Threading.Tasks;
using Gunnsoft.Cqs.Events;

namespace Gunnsoft.Cqs.Commands
{
    public interface ICommandHandler<in TCommand, TEvent>
        where TCommand : ICommand<TEvent>
        where TEvent : IEvent
    {
        Task<TEvent> HandleAsync(TCommand command, CancellationToken cancellationToken);
    }
}