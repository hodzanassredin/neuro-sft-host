using System.Text.Json;

namespace LlmCommon.Transport
{
    public class Envelope
    {
        public Envelope()
        {

        }

        public Envelope(Ids.Id corellationId, object obj)
        {
            CorellationId = corellationId ?? throw new ArgumentNullException(nameof(corellationId));
            if (obj != null) {
                Payload = JsonSerializer.Serialize(obj, obj.GetType());
                Type = obj.GetType().FullName!;
            }
        }

        public string Payload { get; set; }
        public string Type { get; set; }
        public Ids.Id CorellationId { get; set; }

        public object? Get()
        {
            if (String.IsNullOrEmpty(Payload)) {
                return null;
            }

            var type = typeof(Envelope).Assembly.GetType(Type);
            if (type == null) throw new ArgumentOutOfRangeException("type");
            return JsonSerializer.Deserialize(Payload, type);
        }

        public T? Get<T>()
        {
            if (String.IsNullOrEmpty(Payload))
            {
                return default;
            }
            var type = typeof(Envelope).Assembly.GetType(Type);
            if (type == null || !type.IsAssignableTo(typeof(T))) throw new ArgumentOutOfRangeException("type");
            return (T)JsonSerializer.Deserialize(Payload, type);
        }
    }
}
