
namespace LlmCommon.Abstractions
{
    public interface IRequestHandler
    {
        Task Handle(Command cmd);
        Task<TV> HandleQuery<TV>(Query<TV> query) where TV : View;

        bool IsConnected { get; }
    }
}
