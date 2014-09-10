﻿using System;
using System.Collections.Generic;
using CallWall.Web.Contracts;
using CallWall.Web.Contracts.Contact;

namespace CallWall.Web.Providers
{
    public interface IAccountContactProvider
    {
        string Provider { get; }

        //TODO: Change to return Task<IFeed<IAccountContactSummary>>
        IObservable<IFeed<IAccountContactSummary>> GetContactsFeed(IAccountData account, DateTime lastUpdated);

        IObservable<IContactProfile> GetContactDetails(IEnumerable<ISession> session, string[] contactKeys);
    }
}