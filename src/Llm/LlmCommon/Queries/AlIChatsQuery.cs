

using LlmCommon.Abstractions;
using LlmCommon.Views;

namespace LlmCommon.Queries
{
    public class AllChatsQuery : Query<AllChatsView>
    {
        public static readonly AllChatsQuery Instance = new AllChatsQuery();

        public override Task<AllChatsView> Accept(IViewStorage visitor)
        {
            return visitor.Get(this);
        }
    }
}
