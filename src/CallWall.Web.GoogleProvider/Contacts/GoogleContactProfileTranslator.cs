using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CallWall.Web.Domain;

namespace CallWall.Web.GoogleProvider.Contacts
{
    internal sealed class GoogleContactProfileTranslator : IGoogleContactProfileTranslator
    {
        private static readonly Regex FullDobRegex = new Regex(@"(?<year>\d{4})-(?<month>\d\d)-(?<day>\d\d)");
        private static readonly Regex YearlessDobRegex = new Regex(@"--(?<month>\d\d)-(?<day>\d\d)");

        
        public Dictionary<string, string> ToGroupDictionary(string response)
        {
            var xDoc = XDocument.Parse(response);
            var xGroupFeed = xDoc.Root;
            if (xGroupFeed == null)
                return new Dictionary<string, string>();

            var groups =
                (
                    from groupEntry in xGroupFeed.Elements("x", "entry")
                    let id = groupEntry.Element("x", "id")
                    let title = groupEntry.Element("x", "title")
                    where id != null && title != null && !string.IsNullOrWhiteSpace(title.Value)
                    select new { Id = id.Value, Title = title.Value.Replace("System Group: ", string.Empty) }
                ).ToDictionary(g => g.Id, g => g.Title);

            return groups;
        }

        public BatchOperationPage<IAccountContactSummary> Translate(string response, string accessToken, IAccount account, Dictionary<string, string> groups)
        {
            //response can be non xml i.e. "Temporary problem - please try again later and consider using batch operations. The user is over quota."
            var xDoc = XDocument.Parse(response);
            if (xDoc.Root == null)
                return null;

            var entries = xDoc.Root.Elements(XmlEx.Atom.Entry);
            var contacts = new List<IAccountContactSummary>();
            foreach (var xContactEntry in entries)
            {
                if (xContactEntry == null)
                    return null;

                var providerId = GetId(xContactEntry);
                if (IsDeleted(xContactEntry))
                {
                    contacts.Add(new DeletedContactSummary(providerId, account.AccountId));
                }
                else
                {
                    var title = GetTitle(xContactEntry);
                    var fullName = GetFullName(xContactEntry);
                    var dateOfBirth = GetDateOfBirth(xContactEntry);
                    var avatar = GetAvatar(xContactEntry, accessToken);
                    var tags = GetTags(xContactEntry, groups).ToArray();
                    var handles = GetHandles(xContactEntry).ToArray();
                    var organizations = GetOrganizations(xContactEntry).ToArray();
                    var relationships = GetRelationships(xContactEntry).ToArray();

                    //TODO: Need to converge on a std naming AccountId==AcountUserName?! -LC
                    var contact = new ContactSummary(providerId, account.AccountId, 
                        title,fullName, dateOfBirth, 
                        avatar, 
                        tags, handles, organizations, relationships);
                    contacts.Add(contact);
                }
            }


            var totalResults = xDoc.Root.Element(XmlEx.OpenSearch.TotalResults);
            var startIndex = xDoc.Root.Element(XmlEx.OpenSearch.StartIndex);
            var itemsPerPage = xDoc.Root.Element(XmlEx.OpenSearch.ItemsPerPage);
            if (startIndex == null || itemsPerPage == null || totalResults == null)
                return new BatchOperationPage<IAccountContactSummary>(contacts, 0, 1, -1);

            return new BatchOperationPage<IAccountContactSummary>(contacts,
                int.Parse(startIndex.Value),
                int.Parse(totalResults.Value),
                int.Parse(itemsPerPage.Value));
        }

        

        
        private static string ToContactAssociation(XAttribute relAttribute)
        {
            //Could be a look
            if (relAttribute == null || relAttribute.Value == null)
                return "Other";
            var hashIndex = relAttribute.Value.LastIndexOf("#", StringComparison.Ordinal);
            var association = relAttribute.Value.Substring(hashIndex + 1);
            return PascalCase(association);
        }

        private static bool IsDeleted(XElement xContactEntry)
        {
            //Check from presence of a <gd:deleted/> element. Not sure what its contents will be.
            return xContactEntry.XPathSelectElements("gd:deleted", XmlEx.Ns).Any();
        }

        private static string GetId(XElement xContactEntry)
        {
            return XPathString(xContactEntry, "x:id", XmlEx.Ns);
        }

        private static string GetTitle(XElement xContactEntry)
        {
            var title = XPathString(xContactEntry, "x:title", XmlEx.Ns);
            if (string.IsNullOrWhiteSpace(title))
                title = XPathString(xContactEntry, "gd:name/gd:fullName", XmlEx.Ns);
            if (string.IsNullOrWhiteSpace(title))
            {
                var givenName = XPathString(xContactEntry, "gd:name/gd:givenName", XmlEx.Ns);
                var familyName = XPathString(xContactEntry, "gd:name/gd:familyName", XmlEx.Ns);
                title = string.Format("{0} {1}", givenName, familyName).Trim();
            }
            if (string.IsNullOrWhiteSpace(title))
                title = GetEmailAddresses(xContactEntry).Select(ch => ch.Handle).FirstOrDefault();
            return title;
        }

        private static string GetFullName(XElement xContactEntry)
        {
            /*
             * <gd:name>
                   <gd:fullName>Dr John Marks</gd:fullName>
                   <gd:namePrefix>Dr</gd:namePrefix>
                   <gd:givenName>John</gd:givenName>
                   <gd:additionalName>Western Ensenada Motor</gd:additionalName>
                   <gd:familyName>Marks</gd:familyName>
                   <gd:nameSuffix>(sladehastings@me.com)</gd:nameSuffix>
             */

            var nameParts = new[]
                {
                    "gd:name/gd:namePrefix",
                    "gd:name/gd:givenName",
                    "gd:name/gd:additionalName",
                    "gd:name/gd:familyName",
                    "gd:name/gd:nameSuffix"
                }
                .Select(namePart => XPathString(xContactEntry, namePart, XmlEx.Ns))
                .Where(val => !string.IsNullOrWhiteSpace(val));
        
            return string.Join(" ", nameParts).Trim();
        }

        
        private static IAnniversary GetDateOfBirth(XElement xContactEntry)
        {
            //<gContact:birthday when="1979-06-01"/>
            var xBirthday = xContactEntry.Elements("gContact", "birthday")
                .Select(x => x.Attribute("when"))
                .FirstOrDefault(att => att != null);

            if (xBirthday == null)
                return null;

            var dobMatch = FullDobRegex.Match(xBirthday.Value);
            if (dobMatch.Success)
            {
                var year = int.Parse(dobMatch.Groups["year"].Value);
                var month = int.Parse(dobMatch.Groups["month"].Value);
                var day = int.Parse(dobMatch.Groups["day"].Value);
                return new Anniversary(year, month, day);
            }

            dobMatch = YearlessDobRegex.Match(xBirthday.Value);
            if (dobMatch.Success)
            {
                var month = int.Parse(dobMatch.Groups["month"].Value);
                var day = int.Parse(dobMatch.Groups["day"].Value);
                return new Anniversary(month, day);
            }
            return null;
        }

        private static IEnumerable<ContactHandle> GetHandles(XElement xContactEntry)
        {
            return GetEmailAddresses(xContactEntry)
                .Concat(GetPhoneNumbers(xContactEntry));

        }

        private static IEnumerable<ContactHandle> GetEmailAddresses(XElement xContactEntry)
        {
            //<gd:email rel='http://schemas.google.com/g/2005#home' address='bob@gmail.com' primary='true'/>
            //<gd:email rel="http://schemas.google.com/g/2005#other" address="bob2@gmail.com" />
            //TODO: Return "Other" email qualifier as null. -LC
            //TODO: Perf test using xContactEntry.XPathSelectElements("gd:email", Ns) vs xContactEntry.Elements(Gd.Email) -LC
            var emails = from xElement in xContactEntry.XPathSelectElements("gd:email", XmlEx.Ns)
                         //var emails = from xElement in xContactEntry.Elements(Gd.Email)
                         orderby (xElement.Attribute("primary") != null) descending
                         select new ContactEmailAddress(xElement.Attribute("address").Value, StripAnchor(xElement.Attribute("rel")));
            return emails;
        }

        private static IEnumerable<ContactHandle> GetPhoneNumbers(XElement xContactEntry)
        {
            //<gd:phoneNumber rel="http://schemas.google.com/g/2005#mobile" primary="true" uri="tel:+61-400-006-024">+61400006024</gd:phoneNumber>
            //<gd:phoneNumber rel="http://schemas.google.com/g/2005#mobile" uri="tel:+44-7766-558031">+44 77 66 55 8031</gd:phoneNumber>
            //<gd:phoneNumber rel="http://schemas.google.com/g/2005#mobile" primary="true">07816881423</gd:phoneNumber>

            //TODO: Prefer Uri (with "tel:" removed) else use Element value -LC
            var phoneNumbers = from xElement in xContactEntry.XPathSelectElements("gd:phoneNumber", XmlEx.Ns)
                               orderby (xElement.Attribute("primary") != null) descending
                               select new ContactEmailAddress(
                                   ExtractTelephoneNumber(xElement),
                                   StripAnchor(xElement.Attribute("rel")));
            return phoneNumbers;
        }

        private static IEnumerable<IContactAssociation> GetOrganizations(XElement xContactEntry)
        {
            /*<gd:organization rel='http://schemas.google.com/g/2005#work'><gd:orgName>Technip</gd:orgName></gd:organization>*/
            //Or
            /*<gd:organization rel=\"http://schemas.google.com/g/2005#other\"><gd:orgName>Technip</gd:orgName><gd:orgTitle>Executive Director</gd:orgTitle></gd:organization>*/
            var organizations = from xElement in xContactEntry.XPathSelectElements("gd:organization", XmlEx.Ns)
                                where xElement.XPathSelectElement("gd:orgName", XmlEx.Ns) != null
                                select ExtractOrganization(xElement);
                                
            return organizations;
        }

        private static IContactAssociation ExtractOrganization(XElement xElement)
        {
            var orgName = xElement.XPathSelectElement("gd:orgName", XmlEx.Ns).Value;
            var xTitle = xElement.XPathSelectElement("gd:orgTitle", XmlEx.Ns);
            if (xTitle != null)
            {
                return new ContactAssociation(orgName, xTitle.Value);
            }
            return new ContactAssociation(xElement.Attribute("rel").Value, orgName);
        }

        private static string ExtractTelephoneNumber(XElement phoneXElement)
        {
            if (phoneXElement.Attribute("uri") != null)
            {
                //uri="tel:+44-7766-558031"
                var unparsedUri = phoneXElement.Attribute("uri").Value;
                if (!string.IsNullOrWhiteSpace(unparsedUri))
                {
                    var schemeEndIdx = unparsedUri.IndexOf(':');
                    return unparsedUri.Substring(schemeEndIdx + 1);
                }
            }
            return phoneXElement.Value;
        }

        private static string StripAnchor(XAttribute attribute)
        {
            if (attribute != null && !string.IsNullOrWhiteSpace(attribute.Value))
            {
                var hashIdx = attribute.Value.LastIndexOf('#');
                if (hashIdx > -1)
                {
                    return attribute.Value.Substring(hashIdx + 1);
                }
            }
            return null;
        }

        private static string GetAvatar(XElement xContactEntry, string accessToken)
        {
            var googleAvatar = xContactEntry.Elements("x", "link")
                                .Where(x => x.Attribute("rel") != null
                                            && x.Attribute("rel").Value == "http://schemas.google.com/contacts/2008/rel#photo"
                                            && x.Attribute("type") != null
                                            && x.Attribute("type").Value == "image/*"
                                            && x.Attribute("href") != null
                                            && x.Attribute(XmlEx.Gd.ETag) != null)    //The absence of an etag attribute means that there is no image content --https://groups.google.com/forum/#!topic/google-contacts-api/bbIf5tcvhU0
                                .Select(x => x.Attribute("href"))
                                .Where(att => att != null)
                                .Select(att => att.Value + "?access_token=" + accessToken)
                                .FirstOrDefault();
            return googleAvatar;
        }
        
        private static string XPathString(XNode source, string expression, IXmlNamespaceResolver ns)
        {
            var result = source.XPathSelectElement(expression, ns);
            return result == null ? null : result.Value;
        }

        private static string PascalCase(string input)
        {
            var head = input.Substring(0, 1).ToUpperInvariant();
            var tail = input.Substring(1);
            return head + tail;
        }






        //TODO: Implement the following from legacy code

        private static IEnumerable<IContactAssociation> GetRelationships(XElement xContactEntry)
        {
            //<gContact:relation rel='partner'>Anne</gContact:relation>
            var relationships = from xElement in xContactEntry.XPathSelectElements("gContact:relation", XmlEx.Ns)
                                select new ContactAssociation(ToContactAssociation(xElement.Attribute("rel")), xElement.Value);
            return relationships;
        }

        private static IEnumerable<string> GetTags(XElement xContactEntry, Dictionary<string, string> groupLookup)
        {
            return from groupUri in GetGroups(xContactEntry)
                   select groupLookup[groupUri];

        }
        private static IEnumerable<string> GetGroups(XElement xContactEntry)
        {
            var groupUris = from xElement in xContactEntry.XPathSelectElements("gContact:groupMembershipInfo", XmlEx.Ns)
                            let hrefAttribute = xElement.Attribute("href")
                            where hrefAttribute != null
                            select hrefAttribute.Value;
            return groupUris;
        }

    }
}