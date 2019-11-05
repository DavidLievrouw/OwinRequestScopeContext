@echo off
cls
SET DIR=%~dp0%
SET SRCDIR=./src
SET PRODUCT=OwinRequestScopeContext

dotnet test %SRCDIR%/%PRODUCT%.Tests/%PRODUCT%.Tests.csproj

pause