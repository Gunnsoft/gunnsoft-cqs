using Gunnsoft.Cqs.Events;

namespace Gunnsoft.Cqs.Commands
{
    public interface ICommand<TEvent>
        where TEvent : IEvent
    {
    }
}