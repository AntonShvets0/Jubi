using Jubi.Updates;

namespace Jubi.Abstracts
{
    public abstract class EventHandler
    {
        public SiteProvider SiteProvider { get; set; }

        public abstract bool IsAvailable(object data);
        public abstract void Handle(User initiator, object data);
    }
    
    
    public abstract class EventHandler<T> : EventHandler
        where T : class, IUpdateContent
    {
        public override bool IsAvailable(object data) => data is T;

        public override void Handle(User initiator, object data) => Handle(initiator, data as T);

        public abstract void Handle(User initiator, T data);
    }
}