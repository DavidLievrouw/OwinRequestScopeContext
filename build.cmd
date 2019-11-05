@echo off
cls
SET DIR=%~dp0%
SET SRCDIR=./src
SET PRODUCT=OwinRequestScopeContext

dotnet publish %SRCDIR%/%PRODUCT%/%PRODUCT%.csproj --framework netstandard2.0 --configuration Release --output "bin/Publish/netstandard2.0" /p:IsPublishing="true"
dotnet publish %SRCDIR%/%PRODUCT%/%PRODUCT%.csproj --framework net461 --configuration Release --output "bin/Publish/net461" /p:IsPublishing="true"
