using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.GoogleProvider.Providers.Contacts
{
    interface IGoogleContactProfileTranslator
    {
        Dictionary<string, string> ToGroupDictionary(string response);

        BatchOperationPage<IAccountContactSummary> Translate(string response, string accessToken, IAccount account,
            Dictionary<string, string> groups);
    }
}