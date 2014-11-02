REM Install Chocolatey
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'))" && SET PATH=%PATH%;%ALLUSERSPROFILE%\chocolatey\bin


REM Install EventStore to local directory (getEventStore.com)
REM if not exist ..\server\EventStore\ MD ..\server\EventStore\
REM choco install eventstore -version 3.0.0.3 -installArgs '..\server\EventStore\'
choco install eventstore -version 3.0.0.3 -installArgs 'C:\Users\Lee\Documents\GitHub\CallWall.Web\server\EventStore'