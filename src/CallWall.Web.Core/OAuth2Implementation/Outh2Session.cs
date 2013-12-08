﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CallWall.Web.OAuth2Implementation
{
    public sealed class Session : ISession
    {
        //NOTE : other than Provider this is athe same as the google session and is really just seems to be a description of OAuth 2
        private readonly string _accessToken;
        private readonly string _refreshToken;
        private readonly DateTimeOffset _expires;
        private readonly ISet<string> _authorizedResources;
        private readonly string _provider;
        private readonly IAccount _accountDetails;

        public Session(string accessToken, string refreshToken, TimeSpan accessPeriod, DateTimeOffset requested, IEnumerable<string> authorizedResources, IAccount accountDetails)
            : this(accessToken, refreshToken, requested + accessPeriod, authorizedResources, accountDetails)
        {
        }

        public Session(string accessToken, string refreshToken, DateTimeOffset expires, IEnumerable<string> authorizedResources, IAccount accountDetails)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _expires = expires;
            _provider = provider;
            _accountDetails = accountDetails;
            _authorizedResources = new HashSet<string>(authorizedResources);
        }

        public string Provider { get { return _provider; } }
        public string AccessToken { get { return _accessToken; } }
        public string RefreshToken { get { return _refreshToken; } }
        public DateTimeOffset Expires { get { return _expires; } }

        public bool HasExpired()
        {
            return DateTimeOffset.Now > _expires;
        }

        public IAccount AccountDetails { get { return _account; } }

        public ISet<Uri> AuthorizedResources
        public ISet<string> AuthorizedResources
        {
            get { return _authorizedResources; }
        }

        public string Serialize()
        {
            var jObject = JObject.FromObject(this);
            var json = jObject.ToString(Formatting.None);
            return json;
        }

        public override string ToString()
        {
            return string.Format("Session {{ AccessToken : '{0}', RefreshToken : '{1}', Expires : '{2:o}', AuthorizedResources : '{3}', AccountDetails : '{4}' }}", AccessToken, RefreshToken, Expires, string.Join(";", AuthorizedResources), AccountDetails);
        }
    }
}