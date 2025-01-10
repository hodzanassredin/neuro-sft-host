using LlmCommon.Abstractions;

namespace LlmCommon
{
    public abstract class Query
    {
        public Ids.Id Id { get; set; } = Ids.dir.GenerateId();
        public abstract Task<View> Accept(ViewStorage visitor);
    }

    public abstract class TypedQuery<V> : Query where V : View { } 
}
