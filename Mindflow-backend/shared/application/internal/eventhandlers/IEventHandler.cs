using Mindflow_backend.Shared.Domain.Model.Events;
using Cortex.Mediator.Notifications;

namespace Mindflow_backend.Shared.Application.Internal.EventHandlers;

public interface IEventHandler<in TEvent> : INotificationHandler<TEvent> where TEvent : IEvent
{
}
