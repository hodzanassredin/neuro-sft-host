
namespace LlmCommon.Abstractions
{
    public interface IEventHandler
    {
        Task Handle(Event ev);
    }
}
