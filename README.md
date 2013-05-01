CallWall.Web
============

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
