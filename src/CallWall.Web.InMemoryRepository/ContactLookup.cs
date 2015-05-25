using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CallWall.Web.Domain;

namespace CallWall.Web.InMemoryRepository
{
    class ContactLookup
    {
        private readonly Guid _userId;
        private readonly IDictionary<int, ContactProfile> _contactsById = new Dictionary<int, ContactProfile>();
        private readonly IDictionary<string, List<IContactProfile>> _contactsByKey = new Dictionary<string, List<IContactProfile>>();

        public ContactLookup(Guid userId)
        {
            _userId = userId;
        }

        public Guid UserId { get { return _userId; } }

        public ContactLookup Add(ContactAggregateUpdate update)
        {
            ContactProfile contact;
            if (_contactsById.TryGetValue(update.Id, out contact))
            {
                if (update.IsDeleted)
                {
                    Remove(update, contact);
                    return this;
                }
                ApplyUpdate(update, contact);
            }
            else
            {
                contact = new ContactProfile(update.Id);
                ApplyUpdate(update, contact);
                _contactsById[contact.Id] = contact;
            }
            IndexHandles(update, contact);

            return this;
        }

        public IContactProfile GetById(int id)
        {
            return _contactsById[id];
        }

        public IEnumerable<IContactProfile> GetByContactKeys(string[] contactKeys)
        {
            Trace.WriteLine("---GetByContactKeys([" + string.Join("], [", contactKeys) + "])");
            return from key in contactKeys
                   from contact in _contactsByKey.GetOrDefault(key, _=>new List<IContactProfile>())
                   group contact by contact into distinctContacts
                   orderby distinctContacts.Count()
                   select distinctContacts.Key;
        }
        
        private static void ApplyUpdate(ContactAggregateUpdate update, ContactProfile contact)
        {
            if (!string.IsNullOrEmpty(update.NewTitle))
                contact.Title = update.NewTitle;
            //if (!string.IsNullOrEmpty(update.NewFullName))
            //    contact.FullName = update.NewFullName;
            if (update.RemovedAvatars != null)
                update.RemovedAvatars.ToList().ForEach(avatar => contact.AvatarUris.Remove(avatar));
            if (update.RemovedHandles != null)
                update.RemovedHandles.ToList().ForEach(handle => contact.Handles.Remove(handle));
            //if(update.RemovedProviders != null)
            //    update.RemovedProviders.ToList().ForEach(provider=> contact.Providers.Remove(provider));
            if (update.RemovedTags != null)
                update.RemovedTags.ToList().ForEach(tag => contact.Tags.Remove(tag));
            if (update.RemovedOrganizations != null)
                update.RemovedOrganizations.ToList().ForEach(org => contact.Organizations.Remove(org));
            if (update.RemovedRelationships != null)
                update.RemovedRelationships.ToList().ForEach(org => contact.Relationships.Remove(org));

            if (update.AddedAvatars != null)
                contact.AvatarUris.AddRange(update.AddedAvatars);
            if (update.AddedHandles != null)
                contact.Handles.AddRange(update.AddedHandles.Select(h => h.ToContactHandle()));
            //if(update.AddedProviders != null)
            //    contact.Providers.AddRange(update.AddedProviders);
            if (update.AddedTags != null)
                contact.Tags.AddRange(update.AddedTags);
            if (update.AddedOrganizations != null)
                update.AddedOrganizations.ToList().ForEach(org => contact.Organizations.Add(org));
            if (update.AddedRelationships != null)
                update.AddedRelationships.ToList().ForEach(org => contact.Relationships.Add(org));
        }

        private void IndexHandles(ContactAggregateUpdate update, ContactProfile contact)
        {
            if (update.RemovedHandles != null)
                DeleteHandleIndex(update.RemovedHandles, contact);
            if (update.AddedHandles != null)
                AddHandleIndex(update, contact);
        }

        private void Remove(ContactAggregateUpdate update, ContactProfile contact)
        {
            _contactsById.Remove(update.Id);
            DeleteHandleIndex(contact.Handles, contact);
        }

        private void AddHandleIndex(ContactAggregateUpdate update, ContactProfile contact)
        {
            var normalizedHandles = update.AddedHandles
                .Select(h => h.ToContactHandle())
                .SelectMany(h => h.NormalizedHandle())
                .Where(h=>!string.IsNullOrWhiteSpace(h));
            foreach (var handle in normalizedHandles)
            {
                List<IContactProfile> lookup;
                if (!_contactsByKey.TryGetValue(handle, out lookup))
                {
                    lookup = new List<IContactProfile>();
                    Trace.WriteLine("---Adding Key [" + handle + "]");
                    _contactsByKey[handle] = lookup;
                }
                else
                {
                    Trace.WriteLine(string.Format("Found duplicate key : '{0}'", handle));
                }
                lookup.Add(contact);
            }
        }

        private void DeleteHandleIndex(IEnumerable<ContactHandle> handles, ContactProfile contact)
        {
            foreach (var handle in handles.SelectMany(h => h.NormalizedHandle()))
            {
                var lookup = _contactsByKey[handle];
                lookup.Remove(contact);
                if (lookup.Count == 0)
                {
                    _contactsByKey.Remove(handle);
                }
            }
        }
    }
}