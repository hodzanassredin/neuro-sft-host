
using LlmCommon.Abstractions;

namespace LlmCommon
{


    public abstract class Command
    {
        public Ids.Id Id { get; set; } = Ids.dir.GenerateId();

        public abstract Task Accept(IExecutor visitor, IContext ctx);
    }
}
