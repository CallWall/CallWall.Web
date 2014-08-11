﻿using CallWall.Web.Providers;
using CallWall.Web.Account;

namespace CallWall.Web.GoogleProviderFake
{
    public sealed class FakeGoogleModule : IModule
    {
        public void Initialise(ITypeRegistry registry)
        {
            registry.RegisterType<IAccountContactProvider, FakeGoogleAccountContactProvider>();
            registry.RegisterType<ICommunicationProvider, FakeGoogleCommunicationProvider>();
            registry.RegisterType<ICalendarProvider, FakeGoogleCalendarProvider>();
            registry.RegisterType<IGalleryProvider, FakeGoogleGalleryProvider>();
            registry.RegisterType<IContactCollaborationProvider, FakeGoogleContactCollaborationProvider>(); 
            registry.RegisterType<IAccountAuthentication, FakeGoogleAuthentication>("FakeGoogleAuthentication");
        }
    }
}
