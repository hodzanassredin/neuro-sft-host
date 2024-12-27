

using LlmCommon.Abstractions;

namespace LlmCommon.Queries
{
    public class AllChatsQuery : Query
    {
        public static readonly AllChatsQuery Instance = new AllChatsQuery();

        public override Task<View> Accept(IViewStorage visitor)
        {
            return visitor.Get(this);
        }
    }
}
