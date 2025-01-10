

using LlmCommon.Abstractions;
using LlmCommon.Views;

namespace LlmCommon.Queries
{
    public class AllChatsQuery : TypedQuery<AllChatsView>
    {
        public static readonly AllChatsQuery Instance = new AllChatsQuery();

        public override async Task<View> Accept(ViewStorage visitor)
        {
            return await visitor.Get(this);
        }
    }
}
