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
By enabling cookies (Remember me) by simply hitting the browser with the querystring specifying the contact id (Phone number or email address etc)



Web host for the CallWall product. Hosts the Cloud client and any downloads



#Context Glass

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
