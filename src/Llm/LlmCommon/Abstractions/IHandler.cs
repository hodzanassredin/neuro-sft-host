using LlmCommon.Transport;

namespace LlmCommon.Abstractions
{
    public interface IHandler
    {
        Task<Output> Handle(Command cmd);
        Task<OutputWithPayload<TV>> HandleQuery<TV>(Query<TV> query) where TV : View;
    }
}
