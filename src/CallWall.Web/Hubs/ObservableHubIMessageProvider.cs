using System;
using System.Collections.Generic;
using CallWall.Web.Contracts;
using CallWall.Web.Contracts.Communication;
using CallWall.Web.GoogleProvider.Providers.Gmail;

namespace CallWall.Web.Hubs
{
    class ObservableHubIMessageProvider : IObservableHubDataProvider<IMessage>
    {
        private readonly ICommunicationQueryProvider _communicationQueryProvider;//IEnumerable?

        public ObservableHubIMessageProvider(ICommunicationQueryProvider communicationQueryProvider)
        {
            _communicationQueryProvider = communicationQueryProvider;
        }

        public IObservable<IMessage> GetObservable()
        {
            return _communicationQueryProvider.LoadMessages(new DummyProfile("lee.ryan.campbell@gmail.com"));//TODO harcoded value can go once the interface allows some sort of param
        }
    }
    internal class DummyProfile : IProfile
    {
        public DummyProfile(string identifier)
        {
            Identifiers = new IPersonalIdentifier[] { new PersonalIdentifier("?", identifier, GmailProviderDescription.Instance) };
        }

        public IList<IPersonalIdentifier> Identifiers { get; private set; }
    }
}