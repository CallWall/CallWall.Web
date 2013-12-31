/// <reference path="../knockout-3.0.0.debug.js" />
/// <reference path="../Contacts.Observable.SignalR.js" />
/// <reference path="../jasmine/jasmine.js" />

describe("Contacts", function () {
    var anoncontactSvg = '/Content/images/AnonContact.svg';

    describe("ContactViewModel", function () {
        var contact, contactViewModel;

        beforeEach(function () {
            contact = { Title: 'abc', Tags: ['alpha', 'beta'] };
            contactViewModel = new ContactViewModel(contact);
        });

        it("should be able to created with a valid contact", function () {
            expect(contactViewModel).toBeDefined();
            expect(contactViewModel.titleUpperCase).toBe('ABC');
            expect(contactViewModel.primaryAvatar).toBe(anoncontactSvg);
            expect(contactViewModel.isVisible()).toBeTruthy();
            expect(contactViewModel.tags).toEqual(contact.Tags);
        });
        
        describe("filter", function () {
            it("should be visible when filter text is start of title", function () {
                var validFilters = ['A', 'AB', 'ABC'];
                validFilters.forEach(function(prefixText) {
                    contactViewModel.filter(prefixText);
                    expect(contactViewModel.isVisible()).toBeTruthy();
                });
            });
            it("should not be visible when filter text is not start of title", function () {
                var invalidFilters = ['ABCD', 'B', ' '];
                invalidFilters.forEach(function (prefixText) {
                    contactViewModel.filter(prefixText);
                    expect(contactViewModel.isVisible()).toBeFalsy();
                });
            });
        });
    });
});