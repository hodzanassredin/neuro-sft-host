using LlmCommon.Abstractions;

namespace LlmCommon
{
    public abstract class Query
    {
        public Ids.Id Id { get; set; } = Ids.dir.GenerateId();
        public abstract Task<View> Accept(IViewStorage visitor);
    }
}
