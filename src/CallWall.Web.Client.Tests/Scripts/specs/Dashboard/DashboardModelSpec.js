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
                    title: 'My title',
                    fullName: 'my fullname',
                    dateOfBirth: '2011-12-31T16:00:00.000Z'
                };
                contactProfile.aggregate(data);
                expect(contactProfile.title()).toBe(data.title);
                expect(contactProfile.fullName()).toBe(data.fullName);
                expect(contactProfile.dateOfBirth().getTime()).toBe(new Date(data.dateOfBirth).getTime());
            });
            it("Aggregate can be called multiple times with deltas", function () {
                var data = {
                    title: 'My title',
                    fullName: 'my fullname',
                    dateOfBirth: '2011-12-31T16:00:00.000Z'
                };
                contactProfile.aggregate(data);
                contactProfile.aggregate({ title: 'My title again' });
                expect(contactProfile.title()).toBe('My title again');

                contactProfile.aggregate({ fullName: 'My fullname again' });
                expect(contactProfile.fullName()).toBe('My fullname again');

                contactProfile.aggregate({ dateOfBirth: '2001-06-06T16:00:00.000Z' });
                expect(contactProfile.dateOfBirth().getTime()).toBe(new Date('2001-06-06T16:00:00.000Z').getTime());
                expect(contactProfile.fullName()).toBe('My fullname again');
                expect(contactProfile.title()).toBe('My title again');
            });
            it("Aggregate concats tags", function () {
                contactProfile.aggregate({ tags: ['tag1'] });
                contactProfile.aggregate({ tags: ['tag2'] });
                expect(contactProfile.tags().length).toBe(2);
                expect(contactProfile.tags()[0]).toBe('tag1');
                expect(contactProfile.tags()[1]).toBe('tag2');
            });
            it("Aggregate concats organizations", function () {
                contactProfile.aggregate({ organizations: [{ name: 'org1', association: 'ass1' }] });
                contactProfile.aggregate({ organizations: [{ name: 'org2', association: 'ass2' }] });
                expect(contactProfile.organizations().length).toBe(2);
                expect(contactProfile.organizations()[0].name).toBe('org1');
                expect(contactProfile.organizations()[0].association).toBe('ass1');
                expect(contactProfile.organizations()[1].name).toBe('org2');
                expect(contactProfile.organizations()[1].association).toBe('ass2');
            });
            it("Aggregate concats relationships", function () {
                contactProfile.aggregate({ relationships: [{ name: 'rel1', association: 'assoc1' }] });
                contactProfile.aggregate({ relationships: [{ name: 'rel2', association: 'assoc2' }] });
                expect(contactProfile.relationships().length).toBe(2);
                expect(contactProfile.relationships()[0].name).toBe('rel1');
                expect(contactProfile.relationships()[0].association).toBe('assoc1');
                expect(contactProfile.relationships()[1].name).toBe('rel2');
                expect(contactProfile.relationships()[1].association).toBe('assoc2');
            });
            it("Aggregate concats phone numbers", function () {
                contactProfile.aggregate({ phoneNumbers: [{ name: 'phone1', association: 'phAssoc1' }] });
                contactProfile.aggregate({ phoneNumbers: [{ name: 'phone2', association: 'phAssoc2' }] });
                expect(contactProfile.phoneNumbers().length).toBe(2);
                expect(contactProfile.phoneNumbers()[0].name).toBe('phone1');
                expect(contactProfile.phoneNumbers()[0].association).toBe('phAssoc1');
                expect(contactProfile.phoneNumbers()[1].name).toBe('phone2');
                expect(contactProfile.phoneNumbers()[1].association).toBe('phAssoc2');
            });
            it("Aggregate concats email addresses", function () {
                contactProfile.aggregate({ emailAddresses: [{ name: 'test1@test.com', association: 'emailAssoc1' }] });
                contactProfile.aggregate({ emailAddresses: [{ name: 'beta@live.com', association: 'emailAssoc2' }] });
                expect(contactProfile.emailAddresses().length).toBe(2);
                expect(contactProfile.emailAddresses()[0].name).toBe('test1@test.com');
                expect(contactProfile.emailAddresses()[0].association).toBe('emailAssoc1');
                expect(contactProfile.emailAddresses()[1].name).toBe('beta@live.com');
                expect(contactProfile.emailAddresses()[1].association).toBe('emailAssoc2');
            });
        });

        describe("communications", function () {
            var communications;

            beforeEach(function () {
                communications = viewModel.communications;
            });
            it('should have values defaulted', function () {
                expect(communications.isProcessing()).toBeTruthy();
                expect(communications.entries().length).toBe(0);
            });
            it('should be able to add communication items', function () {
                var item1 = {
                    timestamp: '2014-02-23T09:23:30.335Z',
                    isOutbound: false,
                    subject: 'My subject 1',
                    content: 'lorum ipsumtastic',
                    provider: { name: 'Provider A', image: 'My image url for this provider' }
                }
                var item2 = {
                    timestamp: '2014-01-23T09:23:30.335Z',
                    isOutbound: true,
                    subject: 'OMG - LOL!!!!111!!!!One!11!',
                    content: 'Look a cat playing the piano! I have no hobbies!',
                    provider: { name: 'Retardfeed', image: 'lolkatz.jpg' }
                }
                communications.add(item1);
                communications.add(item2);
                //TODO - Need to get my expected object matcher from mercury.... RC
                var x1 = communications.entries()[0];
                expect(x1.timestamp.getTime()).toBe(new Date(item1.timestamp).getTime());
                expect(x1.isOutbound).toBe(item1.isOutbound);
                expect(x1.subject).toBe(item1.subject);
                expect(x1.content).toBe(item1.content);
                expect(x1.provider.name).toBe(item1.provider.name);
                expect(x1.provider.imageUrl).toBe(item1.provider.image);

                var x2 = communications.entries()[1];
                expect(x2.timestamp.getTime()).toBe(new Date(item2.timestamp).getTime());
                expect(x2.isOutbound).toBe(item2.isOutbound);
                expect(x2.subject).toBe(item2.subject);
                expect(x2.content).toBe(item2.content);
                expect(x2.provider.name).toBe(item2.provider.name);
                expect(x2.provider.imageUrl).toBe(item2.provider.image);
            });
        });

        describe("calendar", function () {
            var calendar;

            beforeEach(function () {
                calendar = viewModel.calendar;
            });
            it('should have values defaulted', function () {
                expect(calendar.isProcessing()).toBeTruthy();
                expect(calendar.entries().length).toBe(0);
            });
            it('should be able to add calendar items', function () {
                var item1 = {
                    date: '2014-02-23T09:23:30.335Z',
                    title: 'My subject 1'                    
                }
                var item2 = {
                    date: '2012-04-23T09:23:30.335Z',
                    title: 'My title 2'
                }
                calendar.add(item1);
                calendar.add(item2);
                //TODO - Need to get my expected object matcher from mercury.... RC
                var x1 = calendar.entries()[0];
                expect(x1.date.getTime()).toBe(new Date(item1.date).getTime());
                expect(x1.title).toBe(item1.title);
               
                var x2 = calendar.entries()[1];
                expect(x2.date.getTime()).toBe(new Date(item2.date).getTime());
                expect(x2.title).toBe(item2.title);
            });
        });

        describe("gallery", function () {
            var gallery;

            beforeEach(function () {
                gallery = viewModel.gallery;
            });
            it('should have values defaulted', function () {
                expect(gallery.isProcessing()).toBeTruthy();
                expect(gallery.entries().length).toBe(0);
            });
            it('should be able to add gallery items', function () {
                var item1 = {
                    createdDate: '2014-01-23T10:39:31.095Z',
                    lastModifiedDate: '2014-02-23T10:39:31.095Z',
                    title : 'Gallery One',
                    provider : 'Windows Live',
                    imageUrls : ['live.com/image1.png','live.com/image2.gif']
                }
                var item2 = {
                    createdDate: '2012-01-23T10:39:31.095Z',
                    lastModifiedDate: '2012-02-23T10:39:31.095Z',
                    title: 'Fun in the sun',
                    provider: 'Picassa',
                    imageUrls: ['http://imageland.com/myuniqueid.png', 'google.com/images/1']
                }
                gallery.add(item1);
                gallery.add(item2);
                //TODO - Need to get my expected object matcher from mercury.... RC
                var x1 = gallery.entries()[0];
                expect(x1.createdDate.getTime()).toBe(new Date(item1.createdDate).getTime());
                expect(x1.lastModifiedDate.getTime()).toBe(new Date(item1.lastModifiedDate).getTime());
                expect(x1.title).toBe(item1.title);
                expect(x1.provider).toBe(item1.provider);
                expect(x1.imageUrls[0]).toBe(item1.imageUrls[0]);
                expect(x1.imageUrls[1]).toBe(item1.imageUrls[1]);

                var x2 = gallery.entries()[1];
                expect(x2.createdDate.getTime()).toBe(new Date(item2.createdDate).getTime());
                expect(x2.lastModifiedDate.getTime()).toBe(new Date(item2.lastModifiedDate).getTime());
                expect(x2.title).toBe(item2.title);
                expect(x2.provider).toBe(item2.provider);
                expect(x2.imageUrls[0]).toBe(item2.imageUrls[0]);
                expect(x2.imageUrls[1]).toBe(item2.imageUrls[1]);
            });
        });

        describe("collaboration", function () {
            var collaboration;

            beforeEach(function () {
                collaboration = viewModel.collaboration;
            });
            it('should have values defaulted', function () {
                expect(collaboration.isProcessing()).toBeTruthy();
                expect(collaboration.entries().length).toBe(0);
            });
            it('should be able to add collaboration items', function () {
                var item1 = {
                    actionDate: '2014-01-23T10:39:31.095Z',
                    actionPerformed: 'integrated like a boss',
                    title: 'synergy',
                    provider: { name: 'Github', image: 'octocat.jpg' },               
                    isCompleted: true
                }
                var item2 = {
                    actionDate: '2012-01-23T10:39:31.095Z',
                    actionPerformed: 'you sketch, i colour',
                    title: 'cartooning',
                    provider: { name: 'Marvel', image: 'cptUsa.jpg' },
                    isCompleted: false
                }
                collaboration.add(item1);
                collaboration.add(item2);
                //TODO - Need to get my expected object matcher from mercury.... RC
                var x1 = collaboration.entries()[0];
                expect(x1.actionDate.getTime()).toBe(new Date(item1.actionDate).getTime());
                expect(x1.title).toBe(item1.title);
                expect(x1.actionPerformed).toBe(item1.actionPerformed);
                expect(x1.provider.name).toBe(item1.provider.name);
                expect(x1.provider.imageUrl).toBe(item1.provider.image);
                expect(x1.isCompleted).toBe(item1.isCompleted);

                var x2 = collaboration.entries()[1];
                expect(x2.actionDate.getTime()).toBe(new Date(item2.actionDate).getTime());
                expect(x2.title).toBe(item2.title);
                expect(x2.actionPerformed).toBe(item2.actionPerformed);
                expect(x2.provider.name).toBe(item2.provider.name);
                expect(x2.provider.imageUrl).toBe(item2.provider.image);
                expect(x2.isCompleted).toBe(item2.isCompleted);
            });
        });

    });
});