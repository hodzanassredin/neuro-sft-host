

using LlmCommon.Abstractions;
using LlmCommon.Views;

namespace LlmCommon.Queries
{
    public class AllChatsQuery : Query<AllChatsView>
    {
        public override Task<AllChatsView> Accept(IViewStorage visitor)
        {
            return visitor.Visit(this);
        }
    }
}
