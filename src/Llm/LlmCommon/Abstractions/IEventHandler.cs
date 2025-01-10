
namespace LlmCommon.Abstractions
{
    public interface IEventHandler
    {
        Task<bool> Handle(Event ev);//todo do not return true, use state has changed event
    }
}
