/// <reference path="../../knockout-3.0.0.debug.js" />
/// <reference path="../../Inhouse/Dashboard/DashboardModel.js" />
/// <reference path="../../jasmine/jasmine.js" />

describe("Dashboard Models", function () {

    describe("DashboardViewModel", function () {
        var viewModel;

        beforeEach(function () {
            viewModel = new callWall.DashboardViewModel();
        });

        it("should not be null", function () {
            expect(viewModel).toBeDefined();

        });

        describe("contactProfile", function () {
            var contactProfile;

            beforeEach(function () {
                contactProfile = viewModel.contactProfile;
            });

            it("should have values defaulted", function () {
                expect(contactProfile).toBeDefined();

                expect(contactProfile.isProcessing()).toBeTruthy();

                expect(contactProfile.title()).toBe('');
                expect(contactProfile.fullName()).toBe('');
                expect(contactProfile.dateOfBirth()).toBeUndefined();
                // TODO create custom matchers for KO such as toBeEmptyArray
                expect(contactProfile.tags().length).toBe(0);//Empty array
                expect(contactProfile.organizations().length).toBe(0);//Empty array
                expect(contactProfile.relationships().length).toBe(0);//Empty array
                expect(contactProfile.phoneNumbers().length).toBe(0);//Empty array
                expect(contactProfile.emailAddresses().length).toBe(0);//Empty array
            });

            it("Aggregate populates the model on first call", function () {
                var data = {
                    Title: 'My title',
                    FullName: 'my fullname',
                    DateOfBirth: '2011-12-31T16:00:00.000Z'
                };
                contactProfile.aggregate(data);
                expect(contactProfile.title()).toBe(data.Title);
                expect(contactProfile.fullName()).toBe(data.FullName);
                expect(contactProfile.dateOfBirth().getTime()).toBe(new Date(data.DateOfBirth).getTime());
            });
            it("Aggregate can be called multiple times with deltas", function () {
                var data = {
                    Title: 'My title',
                    FullName: 'my fullname',
                    DateOfBirth: '2011-12-31T16:00:00.000Z'
                };
                contactProfile.aggregate(data);
                contactProfile.aggregate({ Title: 'My title again' });
                expect(contactProfile.title()).toBe('My title again');

                contactProfile.aggregate({ FullName: 'My fullname again' });
                expect(contactProfile.fullName()).toBe('My fullname again');

                contactProfile.aggregate({ DateOfBirth: '2001-06-06T16:00:00.000Z' });
                expect(contactProfile.dateOfBirth().getTime()).toBe(new Date('2001-06-06T16:00:00.000Z').getTime());
                expect(contactProfile.fullName()).toBe('My fullname again');
                expect(contactProfile.title()).toBe('My title again');
            });
            it("Aggregate concats tags", function () {
                contactProfile.aggregate({ Tags: ['tag1'] });
                contactProfile.aggregate({ Tags: ['tag2'] });
                expect(contactProfile.tags().length).toBe(2);
                expect(contactProfile.tags()[0]).toBe('tag1');
                expect(contactProfile.tags()[1]).toBe('tag2');
            });
            it("Aggregate concats organizations", function () {
                contactProfile.aggregate({ Organizations: [{ Name: 'org1', Association: 'ass1' }] });
                contactProfile.aggregate({ Organizations: [{ Name: 'org2', Association: 'ass2' }] });
                expect(contactProfile.organizations().length).toBe(2);
                expect(contactProfile.organizations()[0].name).toBe('org1');
                expect(contactProfile.organizations()[0].association).toBe('ass1');
                expect(contactProfile.organizations()[1].name).toBe('org2');
                expect(contactProfile.organizations()[1].association).toBe('ass2');
            });
            it("Aggregate concats relationships", function () {
                contactProfile.aggregate({ Relationships: [{ Name: 'rel1', Association: 'assoc1' }] });
                contactProfile.aggregate({ Relationships: [{ Name: 'rel2', Association: 'assoc2' }] });
                expect(contactProfile.relationships().length).toBe(2);
                expect(contactProfile.relationships()[0].name).toBe('rel1');
                expect(contactProfile.relationships()[0].association).toBe('assoc1');
                expect(contactProfile.relationships()[1].name).toBe('rel2');
                expect(contactProfile.relationships()[1].association).toBe('assoc2');
            });
            it("Aggregate concats phone numbers", function () {
                contactProfile.aggregate({ PhoneNumbers: [{ Name: 'phone1', Association: 'phAssoc1' }] });
                contactProfile.aggregate({ PhoneNumbers: [{ Name: 'phone2', Association: 'phAssoc2' }] });
                expect(contactProfile.phoneNumbers().length).toBe(2);
                expect(contactProfile.phoneNumbers()[0].name).toBe('phone1');
                expect(contactProfile.phoneNumbers()[0].association).toBe('phAssoc1');
                expect(contactProfile.phoneNumbers()[1].name).toBe('phone2');
                expect(contactProfile.phoneNumbers()[1].association).toBe('phAssoc2');
            });
            it("Aggregate concats email addresses", function () {
                contactProfile.aggregate({ EmailAddresses: [{ Name: 'test1@test.com', Association: 'emailAssoc1' }] });
                contactProfile.aggregate({ EmailAddresses: [{ Name: 'beta@live.com', Association: 'emailAssoc2' }] });
                expect(contactProfile.emailAddresses().length).toBe(2);
                expect(contactProfile.emailAddresses()[0].name).toBe('test1@test.com');
                expect(contactProfile.emailAddresses()[0].association).toBe('emailAssoc1');
                expect(contactProfile.emailAddresses()[1].name).toBe('beta@live.com');
                expect(contactProfile.emailAddresses()[1].association).toBe('emailAssoc2');
            });
        });
    });
});