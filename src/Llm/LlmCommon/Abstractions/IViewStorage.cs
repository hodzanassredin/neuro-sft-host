
using LlmCommon.Queries;

namespace LlmCommon.Abstractions
{
    public interface IViewStorage
    {
        Task<View> Get(AllChatsQuery q);
    }
}
