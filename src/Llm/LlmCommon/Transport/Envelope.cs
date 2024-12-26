using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace LlmCommon.Transport
{
    public class Envelope
    {
        private Envelope()
        {

        }

        public Envelope(Ids.Id corellationId, object obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            Payload = JsonSerializer.Serialize(obj, obj.GetType());
            Type = obj.GetType().FullName!;
            CorellationId = corellationId ?? throw new ArgumentNullException(nameof(corellationId));
        }

        public string Payload { get; set; }
        public string Type { get; set; }
        public Ids.Id CorellationId { get; set; }

        public object? Get()
        {
            var type = typeof(Envelope).Assembly.GetType(Type);
            if (type == null) throw new ArgumentOutOfRangeException("type");
            return JsonSerializer.Deserialize(Payload, type);
        }
    }
}
