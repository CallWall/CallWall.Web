namespace CallWall.Web.EventStore.Tests
{
    public abstract class DomainEventState
    {
        public static readonly DomainEventState Idle = new IdleState();
        public static readonly DomainEventState LoadingFromHistory = new LoadingFromHistoryState();
        public static readonly DomainEventState BehindWrites = new BehindWritesState();

        public static readonly DomainEventState Current = new CurrentState();

        private DomainEventState()
        {
        }

        public virtual bool IsListening { get { return false; } }
        public virtual bool IsProcessing { get { return false; } }
        public virtual bool IsReplaying { get { return false; } }

        private sealed class LoadingFromHistoryState : DomainEventState
        {
            public override bool IsListening { get { return true; } }
            public override bool IsProcessing { get { return true; } }
            public override bool IsReplaying { get { return true; } }
        }

        private sealed class BehindWritesState : DomainEventState
        {
            public override bool IsListening { get { return true; } }
            public override bool IsProcessing { get { return true; } }
        }

        private sealed class IdleState : DomainEventState
        {
        }

        private sealed class CurrentState : DomainEventState
        {
            public override bool IsListening { get { return true; } }
        }

    }
}