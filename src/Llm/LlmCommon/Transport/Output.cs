namespace LlmCommon.Transport
{
    public class Output
    {
        public Output(Ids.Id requestId, int code = 200, string? error = null)
        {
            RequestId = requestId;
            IsError = code != 200;
            Code = code;
            Error = error;
        }

        public Ids.Id RequestId { get; }
        public int Code { get; }
        public bool IsError { get; set; }
        public string? Error { get; }
    }

    public class OutputWithPayload<T> : Output where T : View
    {
        public OutputWithPayload(Ids.Id requestId, T view, int code = 200, string? error = null) : base(requestId, code, error)
        {
            View = view;
        }

        public T View { get; }
    }
}
