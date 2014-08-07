using System;
using System.Collections.Generic;

namespace CallWall.Web.EventStore.Tests
{
    //CallWall has Users
    //Users have registered Accounts
    //Accounts have a PK of an internally generated integer
    //         have a unique constraint on the composite key of Provider+UserName
    //Accounts have sessions
    //Sessions have authentication and Authorization information
    //         can expire
    //         can be renewed
    //         can be revoked (which will revoke the Account)
    //Accounts have Contacts
    //Contacts have a PK of Provider+AccountId+ProviderId
    //Users have aggregated view of Contacts

    //EventStore        - Just a facade over the Actual Event store (if we need it)
    //UserRepository    - Gets Users. Will replay all over Users event stream into memory to be an internal cache for look ups (Read/Query only?)
    //                  - Find User by Account (e.g. for login)
    //UserFactory       - Creates Users from an Account (doesn't actually do anything but perform some IoC fluff)
    //User              - Can Save/Persist it's changes to the data store (EventStore)    
    //                  - Also loads it's Accounts?
    //Account           - Hmm....Somehow this will have to be able to raise generic events, but internally have provider specific implementations (e.g. for OAuth authentication and renewal)

    

    //internal interface IUserRepository
    //{
    //    User RegisterNewUser(IAccount account);
    //    User FindByAccount(IAccount account);
    //}


    //UserLogin
    //Adding extra accounts
    //Removing accounts
    //Account revoked
    //Merging users


    /*Port any sensible changes from PracticalRx End-2-end back into CallWall

Create set of Acceptance tests to document the expected behaviour

[Acceptance tests]
GIVEN an anonymous user
	WHEN they register (login without the account being registered)
		THEN a User is created
		THEN an Account is created
			With an Account identifier
			With the Provider (gmail/twitter/etc....)
			With permission key set and the mapping to the provider's OAuth Scopes e.g. {Key: "CallWall.Communications", Value: "https://mail.google.com/"}
			(THEN the saved Account is decorated with the available scopes/permissions the User agreed to.)
		THEN the new Account is associated to the new User
		THEN an AccountContact refresh command is issued for the account
		
	WHEN they login (Login with an account that is registered)
		THEN they are logged in as the user the account is associated with	 (implies some sort of look up. Is this in memory? Why not, it should just be a massive hashtable/dictionary.)
		
		
GIVEN a logged in user (i.e. a UserId) with a single account 
	WHEN they request their contacts [From a version/eventId]
		THEN they are returned a subscription to contactUpdates
		THEN they are pushed the contact events loaded in the data store
		
	WHEN user registers an additional account
		THEN an Account is created
			With an Account identifier
			With the Provider (gmail/twitter/etc....)
			With permission key set and the mapping to the provider's OAuth Scopes e.g. {Key: "CallWall.Communications", Value: "https://mail.google.com/"}
		THEN the new Account is associated to the current User
		THEN an AccountContact refresh command is issued for the Account
		
GIVEN a logged in user with a contact updates subscription
	WHEN a contact is added to the data store for the user's account
		THEN they are pushed the contact added event
	WHEN a contact is updated in the data store for the user's account
		THEN they are pushed the contact update event
	WHEN a contact is removed from the data store for the user's account
		THEN they are pushed the contact removal event
	
		
	





GIVEN logged in user removes a registered account
GIVEN logged in user registers an account that is already registered
GIVEN ?? WHEN Account has been revoked from Provider's side

GIVEN a registered account
	WHEN a contact refresh is issued
		THEN new contacts are stored as Contact added events
		THEN updated contacts are stored as Contact updated events
		THEN missing contacts are stored as Contact removal events
     
GIVEN logged in user
     Can request a list of current registered accounts and their providers
     Can request a list of all providers available to register with
     Can change their display name
     Can change their avatar
     
     
     */
}
