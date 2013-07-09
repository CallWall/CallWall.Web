CallWall.Web
============

The CallWall.Web project represents the Website for CallWall.
The website will need to cater for the following:
 1. Marketing site.
 2. ClickOnce downloads (Windows)
 3. Web implementation of CallWall
 4. Settings
 4. Cloud services


## Marketing website 
The marketing site will need to explain to new users what CallWall is via text, images and Video.
It will provide links to download the app for the appropriate installer for the user's current platform.

## Downloads 
User will be able to download the application via the download section on the marketing website.
We will prompt a default download link for the user's current platform. IF the user is viewing on a mobile device, then take them to the appropriate app store.

## User settings
Users are able to login to CallWall with any of their accounts and manage their settings for the application.
User can register for CallWall by simply loging-in in with OAuth.
When registering they get to choose the resources they will allow CallWall to access for each account.
Once logged in, they are able to link accounts e.g. Twitter, Google, FaceBook, Microsoft etc...

## Cloud servcies 
CallWall will provide a hub for users to aggregate their social media contacts together.
The cloud services will provide a common data model for CallWall clients to consume data aggreated across various accounts.
The benefits of using a cloud service instead of having clients connect to the social media service directly include:
 1. Single point of upgrade when 3rd party services change
 2. Allow access to users that would otherwise be blocked by firewalls to access social media data. i.e. Most banks block gmail, Hotmail, FB etc.
 3. Less data for clients to download. As the server can pre filter the data (like large emails & photos) and send the client a compressed feed.
 4. Reduce the number of connections the client needs to make. Client's tend to handle many http conenctions much worse than a server can.
 5. Single code base for social media integration (instead of reproduced in WPF, WinRT, Web, OSX)
 6. Provides a single base for user's to store their settings



## Web implementation 
The Web implementation is effectively the Web UI over the cloud services.
By enabling cookies (Remember me), when navigating the browser to CallWall.com with the querystring specifying the contact id (Phone number or email address etc), the Contact Dashboard can be displayed directly.
This will then allow for very simple triggering systems on clients. For example a small service that accepts push messages (Bluetooth, WebSockets, ISDN, USB, WiDi) and then just open a browser to call wall for the contact.
This will provide a broad reach of systems as simple implementations can just be the service that opens a browser.





### Sequence diagrams

[Authentication Sequence](http://www.websequencediagrams.com/?lz=dGl0bGUgQXV0aGVudGljYXRpb24gU2VxdWVuY2UKCnBhcnRpY2lwYW50IEhvbWVQYWdlAAgNVXNlclNldHRpbmdzACENTG9naW4AMw0iUmVzb3VyY2UgU2VsZWN0b3IiIGFzIAAOCAANCABnDk9BdXRoAEYGAHgILT4AVwU6IApub3RlIG92ZXIAaAY6IENob29zZSBmcm9tIEdvb2dsZSwgVHdpdHRlciwgRkIsIE0kCgCBFAUtPgBnEAA1CHMAMgcgYWNjb3VudCBwcm92aWRlcgBjCwAkGCBzZXJ2aWNlIHRvIHUAeg4gXG5DYWxlbmRhcixFbWFpbCxDb250YWN0cyxQaG90b3MgZXRjLgoAgX0QLT4AgXcKOiBSZWRpcmVjdHMgdG8AgVkHLmNvbQoAghsKLT4AgwUNOk9uIFN1Y2Nlc3MgcgA0CWJhY2sgdG8gQ2FsbFdhbGwK&s=modern-blue)
![Authentication Sequence Image](http://www.websequencediagrams.com/cgi-bin/cdraw?lz=dGl0bGUgQXV0aGVudGljYXRpb24gU2VxdWVuY2UKCnBhcnRpY2lwYW50IEhvbWVQYWdlAAgNVXNlclNldHRpbmdzACENTG9naW4AMw0iUmVzb3VyY2UgU2VsZWN0b3IiIGFzIAAOCAANCABnDk9BdXRoAEYGAHgILT4AVwU6IApub3RlIG92ZXIAaAY6IENob29zZSBmcm9tIEdvb2dsZSwgVHdpdHRlciwgRkIsIE0kCgCBFAUtPgBnEAA1CHMAMgcgYWNjb3VudCBwcm92aWRlcgBjCwAkGCBzZXJ2aWNlIHRvIHUAeg4gXG5DYWxlbmRhcixFbWFpbCxDb250YWN0cyxQaG90b3MgZXRjLgoAgX0QLT4AgXcKOiBSZWRpcmVjdHMgdG8AgVkHLmNvbQoAghsKLT4AgwUNOk9uIFN1Y2Nlc3MgcgA0CWJhY2sgdG8gQ2FsbFdhbGwK&s=modern-blue)

[Linking social media accounts](http://www.websequencediagrams.com/?lz=dGl0bGUgTGlua2luZyBzb2NpYWwgbWVkaWEgYWNjb3VudHMKCnBhcnRpY2lwYW50IFVzZXJTZXR0aW5ncwAMDSJSZXNvdXJjZSBTZWxlY3RvciIgYXMgAA4IAA0IAEANT0F1dGhMb2dpbgoKAE0MIC0-AFwNIDoKbm90ZSBvdmVyAAoPIENob29zZSBmcm9tIEdvb2dsZSwgVHdpdHRlciwgRkIsIE0kAEwQAIEEEAA9CHMAMQgAgXQIIHByb3ZpZGVyAHQLACUYIHNlcnZpY2UgdG8gdQCBCQgAgQIHOyBcbkNvbnRhY3RzLCBUd2VldHMgZXRjLgoAgg8QLT4AggoKOiBSZWRpcmVjdHMgdG8AgU0ILmNvbQoAgi8KAIIbDzpPbiBTdWNjZXNzIHIANQliYWNrIHRvIENhbGxXYWxsAII0GlVzZXIncyB0AIF5D2lzIG5vdyBsaW5rZWQgXG50byB0aGUgQQCCIAd0aGV5IGFyZSBsb2dpbiBpbiBhcyBcbihhbmQgb3RoZXIAMggAhEwIKSA&s=modern-blue)
![Linking social media accounts Image](http://www.websequencediagrams.com/cgi-bin/cdraw?lz=dGl0bGUgTGlua2luZyBzb2NpYWwgbWVkaWEgYWNjb3VudHMKCnBhcnRpY2lwYW50IFVzZXJTZXR0aW5ncwAMDSJSZXNvdXJjZSBTZWxlY3RvciIgYXMgAA4IAA0IAEANT0F1dGhMb2dpbgoKAE0MIC0-AFwNIDoKbm90ZSBvdmVyAAoPIENob29zZSBmcm9tIEdvb2dsZSwgVHdpdHRlciwgRkIsIE0kAEwQAIEEEAA9CHMAMQgAgXQIIHByb3ZpZGVyAHQLACUYIHNlcnZpY2UgdG8gdQCBCQgAgQIHOyBcbkNvbnRhY3RzLCBUd2VldHMgZXRjLgoAgg8QLT4AggoKOiBSZWRpcmVjdHMgdG8AgU0ILmNvbQoAgi8KAIIbDzpPbiBTdWNjZXNzIHIANQliYWNrIHRvIENhbGxXYWxsAII0GlVzZXIncyB0AIF5D2lzIG5vdyBsaW5rZWQgXG50byB0aGUgQQCCIAd0aGV5IGFyZSBsb2dpbiBpbiBhcyBcbihhbmQgb3RoZXIAMggAhEwIKSA&s=modern-blue)



-- Junk notes below--

###Context Glass

##A.A.A  
  * Authenticate clients with the server  
    * For clients (Tablet, PC, Kindle etc.),   
      * [WHEN in Public Context] connect with OAuth to SignalR server/Hub  
          * -->Requires an OAuth implementation of ICredentials?  
      * [WHEN in Private/Work Context] connect with domain credentials?  
          * (run in seperate appdomain?)  
			
    * For Phones (CallWall) dowload the app, connect with OAuth and register the phone number. Server SMS back a code which validates the number.
			
Context Switcher  
  * NFC  
  * WiFi  
  * Dock/Ethernet  
  * Bluetooth X [Can only try/poll, no notification when in range]
	
	
Costs
  * EE 4g 5GB/month is £36 sim only up £50-60 for contract with phone
