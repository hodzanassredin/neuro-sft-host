
using LlmCommon.Queries;
using LlmCommon.Views;

namespace LlmCommon.Abstractions
{
    public interface IViewStorage
    {
        Task<AllChatsView> Get(AllChatsQuery q);
    }
}
