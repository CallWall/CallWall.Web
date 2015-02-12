@ECHO OFF
REM			This file will download EventStore v3.0.1 and install it locally to this git repo.
REM			It assumes the following dependencies are available:
REM				* Powershell																		- http://technet.microsoft.com/en-gb/library/hh847837.aspx
REM				* .NET 4.5 (for Zipping/Unzipping)													- http://www.microsoft.com/en-gb/download/details.aspx?id=42642

SET CurrDir=%~dp0
SET InstallEventStorePsPath=%CurrDir%Install-EventStore.ps1

cd..
SET DownloadUrl=http://download.geteventstore.com/binaries/EventStore-OSS-Win-v3.0.1.zip
SET TargetFolderPath=%cd%\Server\EventStore

:Install
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "& '%InstallEventStorePsPath%' -address '%DownloadUrl%' -destination '%TargetFolderPath%'"
