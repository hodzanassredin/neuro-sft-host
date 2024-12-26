using LlmCommon.Abstractions;

namespace LlmCommon
{
    public abstract class Query<TV> where TV : View
    {
        public Ids.Id Id { get; set; } = Ids.dir.GenerateId();
        public abstract Task<TV> Accept(IViewStorage visitor);
    }
}
