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
        });
    });
});