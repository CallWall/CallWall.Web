namespace CallWall.Web.Domain
{
    public sealed class Event<T>
    {
        private readonly int _eventId;
        private readonly T _value;

        public Event(int eventId, T value)
        {
            _eventId = eventId;
            _value = value;
        }

        public int EventId { get { return _eventId; } }

        public T Value { get { return _value; } }

        public override string ToString()
        {
            return string.Format("EventId:{0}, Value:{1}", EventId, Value);
        }
    }
}