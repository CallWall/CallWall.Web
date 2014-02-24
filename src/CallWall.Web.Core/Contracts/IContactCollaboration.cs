using System;

namespace CallWall.Web.Contracts
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