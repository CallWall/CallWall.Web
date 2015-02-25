using System.IO;

namespace CallWall.Web.GoogleProvider.Providers.Gmail.Imap
{
    public interface IIMapOperation
    {
        bool Execute(string prefix, Stream sendStream, StreamReader receiveStream);
    }
}