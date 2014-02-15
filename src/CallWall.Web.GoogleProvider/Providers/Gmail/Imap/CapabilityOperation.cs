namespace CallWall.Web.GoogleProvider.Providers.Gmail.Imap
{
    internal class CapabilityOperation : ImapOperationBase
    {
        public CapabilityOperation(ILoggerFactory loggerFactory) : base(loggerFactory)
        { }
        protected override string Command
        {
            get { return "CAPABILITY"; }
        }
    }
}