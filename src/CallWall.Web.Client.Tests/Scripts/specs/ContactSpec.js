/// <reference path="../knockout-3.0.0.debug.js" />
/// <reference path="../Contacts.Observable.SignalR.js" />
/// <reference path="../jasmine/jasmine.js" />

describe("Contacts", function () {
  
  it("should be able to create ContactViewModel", function () {
      var contact = { Title : 'abc', Tags : ['alpha', 'beta']};
      var contactViewModel = new ContactViewModel(contact);
      console.log(contactViewModel);
      expect(contactViewModel).toBeDefined();
      expect(contactViewModel.titleUpperCase).toBe('ABC');
      expect(contactViewModel.primaryAvatar).toBe('/Content/images/AnonContact.svg');
      expect(contactViewModel.isVisible).toBeTruthy();
      expect(contactViewModel.tags).toEqual(contact.Tags);

  });
});