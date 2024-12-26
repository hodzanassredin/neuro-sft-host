using System.Text.Json.Serialization;

namespace LlmCommon
{
    public abstract class Entity
    {
        [JsonInclude]
        public bool IsRemoved { get; protected set; }
        [JsonInclude]
        public Ids.Id Id { get; protected set; }
        [JsonInclude]
        public long Version { get; protected set; }

        [JsonIgnore]
        private readonly List<Event> uncommittedEvents = [];

        //for serialization purposes
        protected Entity() { }

        public IEnumerable<Event> DequeueUncommittedEvents()
        {
            var dequeuedEvents = uncommittedEvents.ToList();

            uncommittedEvents.Clear();

            return dequeuedEvents;
        }
        public abstract void Apply(Event @event);
        public virtual void Exec(Event @event) {
            Apply(@event);
            Enqueue(@event);
        }
        private void Enqueue(Event @event)
        {
            Version++;
            uncommittedEvents.Add(@event);
        }
    }
}
