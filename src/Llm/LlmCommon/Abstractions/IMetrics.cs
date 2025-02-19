
namespace LlmCommon.Abstractions
{
    public interface IMetrics
    {
        void IncLlmRequestCount();
        void SetTimeToFirstToken(TimeSpan time);
        void SetTimeToWholeRequest(TimeSpan time);
        void SetTimeToInterTokenDelay(TimeSpan time);
    }
}
