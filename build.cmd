@echo off
cls
SET DIR=%~dp0%
SET SRCDIR=./src/
dotnet publish %SRCDIR%OwinRequestScopeContext/OwinRequestScopeContext.csproj --framework netstandard2.0 --configuration Release --output "bin/Publish/netstandard2.0" /p:IsPublishing="true"
dotnet publish %SRCDIR%OwinRequestScopeContext/OwinRequestScopeContext.csproj --framework net461 --configuration Release --output "bin/Publish/net461" /p:IsPublishing="true"
pause
