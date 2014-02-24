using System;

namespace CallWall.Web.Contracts
{
    public interface IGalleryAlbum
    {
        string[] ImageUrls { get; }
        DateTime CreatedDate { get; }
        DateTime LastModifiedDate { get; }
        string Title { get; }
        string Provider { get; }
    }
}