using System;

namespace CallWall.Web.Domain
{
    public interface IContactCollaboration
    {
        string Title { get; }
        DateTime ActionDate { get; }
        string ActionPerformed { get; }
        bool IsCompleted { get; }
        string Provider { get; }
    }
}