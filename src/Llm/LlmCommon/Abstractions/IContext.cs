
using LlmCommon.Dtos;

namespace LlmCommon.Abstractions
{
    public interface IContext
    {
        User GetCurrentUser();
    }
}
