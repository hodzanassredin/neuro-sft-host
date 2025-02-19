using LlmCommon.Abstractions;
using System.Diagnostics.Metrics;

namespace LlmBackend.Infrastructure
{
    public class MetricsHost : IMetrics
    {
        public const string Name = "Llm";
        static readonly Meter MainMeter = new(Name, "1.0");
        static readonly Counter<int> REQUEST_COUNT = MainMeter.CreateCounter<int>("request_count", "Number of requests received");
        static readonly Histogram<int> REQUEST_LATENCY = MainMeter.CreateHistogram<int>("request_latency_seconds", "s","Request latency in seconds");
        static readonly Histogram<int> TIME_TO_FIRST_TOKEN = MainMeter.CreateHistogram<int>("time_to_first_token_seconds", "s", "Time until first token is received");
        static readonly Histogram<int> INTER_TOKEN_DELAY = MainMeter.CreateHistogram<int>("inter_token_delay_seconds", "s", "Time between tokens");

        public void IncLlmRequestCount()
        {
            REQUEST_COUNT.Add(1);
        }

        public void SetTimeToFirstToken(TimeSpan time)
        {
            TIME_TO_FIRST_TOKEN.Record((int)time.TotalSeconds);
        }

        public void SetTimeToInterTokenDelay(TimeSpan time)
        {
            INTER_TOKEN_DELAY.Record((int)time.TotalSeconds);
        }

        public void SetTimeToWholeRequest(TimeSpan time)
        {
            REQUEST_LATENCY.Record((int)time.TotalSeconds);
        }
    }
}
