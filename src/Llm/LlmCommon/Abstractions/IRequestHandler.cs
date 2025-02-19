
namespace LlmCommon.Abstractions
{
    public interface IRequestHandler
    {
        Task Handle(Command cmd);
        Task<View> HandleQuery(Query query);

        bool IsConnected { get; }
    }
}
