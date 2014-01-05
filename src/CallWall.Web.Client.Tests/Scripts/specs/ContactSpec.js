/// <reference path="../knockout-3.0.0.debug.js" />
/// <reference path="../Inhouse/ContactModels.js" />
/// <reference path="../jasmine/jasmine.js" />

describe("Contacts", function () {
    var anoncontactSvg = '/Content/images/AnonContact.svg';

    describe("ContactViewModel", function () {
        var contact, contactViewModel;

        beforeEach(function () {
            contact = { Title: 'abc', Tags: ['alpha', 'beta'] };
            contactViewModel = new callWall.ContactViewModel(contact);
        });

        it("should be able to created with a valid contact", function () {
            expect(contactViewModel).toBeDefined();
            expect(contactViewModel.titleUpperCase).toBe('ABC');
            expect(contactViewModel.primaryAvatar).toBe(anoncontactSvg);
            expect(contactViewModel.isVisible()).toBeTruthy();
            expect(contactViewModel.tags).toEqual(contact.Tags);
        });
        it("should expose binding properties", function () {
            expect(contactViewModel.isVisible()).toBeDefined();
            expect(contactViewModel.primaryAvatar).toBeDefined();
            expect(contactViewModel.title).toBeDefined();
            expect(contactViewModel.tags).toBeDefined();
        });

        describe("filter", function () {
            it("should be visible when filter text is start of title", function () {
                var validFilters = ['A', 'AB', 'ABC'];
                validFilters.forEach(function (prefixText) {
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
    
    describe("AnyContactGroup", function () {
        var alphaContactGroup;
        beforeEach(function() {
            alphaContactGroup = new callWall.AnyContactGroup('');
        });
        it("should expose binding properties", function() {
            expect(alphaContactGroup.isVisible()).toBeDefined();
            expect(alphaContactGroup.header).toBeDefined();
            expect(alphaContactGroup.contacts()).toBeDefined();
        });
        describe("Is contact valid ", function () {
            it("should always return true", function () {
                var invalidContactTitle = [null, 123, {alpha:'a'},'ABCD', 'a', 'B', 'ax', ' ', ' x', '_x', ':X'];
                invalidContactTitle.forEach(function (title) {
                    expect(alphaContactGroup.isValid({ Title: title })).toBeTruthy();
                });
            });
        });
    });
    
    describe("AlphaContactGroup", function () {
        var groupPrefix = 'X';
        var alphaContactGroup;
        beforeEach(function () {
            alphaContactGroup = new callWall.AlphaContactGroup(groupPrefix);
        });
        it("should expose binding properties", function () {
            expect(alphaContactGroup.isVisible()).toBeDefined();
            expect(alphaContactGroup.header).toBeDefined();
            expect(alphaContactGroup.contacts()).toBeDefined();
        });
        it("should not be visible by default", function () {
            expect(alphaContactGroup.isVisible()).toBeFalsy();
        });
        it("should have no contacts by default", function () {
            expect(alphaContactGroup.contacts().length).toBe(0);
        });
        it("should have no visible contacts by default", function () {
            expect(alphaContactGroup.visibleContacts().length).toBe(0);
        });

        describe("Is contact valid ", function () {
            it("should not be valid when title does not begin with the header", function () {
                var invalidContactTitle = ['ABCD', 'a', 'B', 'ax', ' ', ' x', '_x', ':X'];
                invalidContactTitle.forEach(function (title) {
                    expect(alphaContactGroup.isValid({ Title: title })).toBeFalsy();
                });
            });
            it("should be valid when title does begin with the header", function () {
                var validContactTitle = ['X', 'XYZ', 'xyz', 'x-men', 'Xavier Charles'];
                validContactTitle.forEach(function (title) {
                    expect(alphaContactGroup.isValid({ Title: title })).toBeTruthy();
                });
            });
        });
        describe('Add a contact', function () {
            //What about adding a bad contact - the model doesnt actually deal with this but hopes the caller does
            beforeEach(function () {
                var contact = { Title: 'Xavier Charles', Tags: ['test1', 'beta2'] };
                alphaContactGroup.addContact(contact);
            });
            it("should have 1 contact", function () {
                expect(alphaContactGroup.contacts().length).toBe(1);
            });
            it("should be visible", function () {
                expect(alphaContactGroup.isVisible()).toBeTruthy();
            });
            it("should have 1 visible contact", function () {
                expect(alphaContactGroup.visibleContacts().length).toBe(1);
            });
        });
        describe('Add multiple contacts', function () {
            //What about adding a bad contact - the model doesnt actually deal with this but hopes the caller does
            var contacts;
            beforeEach(function () {
                contacts = [{ Title: 'Xavier Charles', Tags: ['SciFi', 'Legless'] },
                                { Title: 'Xerxes Khan', Tags: ['Fighter', 'Ruler'] },
                                { Title: 'Xylon Forrest', Tags: ['Hippy', 'Dealer'] },
                                { Title: 'Xioping Chang', Tags: ['Mathematician', 'Nerd'] }];
                contacts.forEach(function (contact) {
                    alphaContactGroup.addContact(contact);
                });
            });
            it("should have 4 contact", function () {
                expect(alphaContactGroup.contacts().length).toBe(4);
            });
            it("should be visible", function () {
                expect(alphaContactGroup.isVisible()).toBeTruthy();
            });
            it("should have 4 visible contact", function () {
                expect(alphaContactGroup.visibleContacts().length).toBe(4);
            });
            it("should be sorted alphabetically by Title", function () {
                expect(alphaContactGroup.contacts()[0].title).toBe(contacts[0].Title);
                expect(alphaContactGroup.contacts()[1].title).toBe(contacts[1].Title);
                expect(alphaContactGroup.contacts()[2].title).toBe(contacts[3].Title);
                expect(alphaContactGroup.contacts()[3].title).toBe(contacts[2].Title);
            });
            describe('Filtering on multiple contacts with XA', function () {
                describe('When filter should only match one contact', function () {
                    beforeEach(function () {
                        alphaContactGroup.filter('XA');
                    });
                    it("should have 4 contacts", function () {
                        expect(alphaContactGroup.contacts().length).toBe(4);
                    });
                    it("should be visible", function () {
                        expect(alphaContactGroup.isVisible()).toBeTruthy();
                    });
                    it("should have 1 visible contact", function () {
                        expect(alphaContactGroup.visibleContacts().length).toBe(1);
                    });
                });

                describe('When filter matches no contacts', function () {
                    beforeEach(function () {
                        alphaContactGroup.filter('XAX');
                    });
                    it("should have 4 contacts", function () {
                        expect(alphaContactGroup.contacts().length).toBe(4);
                    });
                    it("should not be visible", function () {
                        expect(alphaContactGroup.isVisible()).toBeFalsy();
                    });
                    it("should have no visible contacts", function () {
                        expect(alphaContactGroup.visibleContacts().length).toBe(0);
                    });
                });
            });
        });
    });
});