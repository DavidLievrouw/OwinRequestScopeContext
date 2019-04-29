@echo off
cls
SET DIR=%~dp0%
SET SRCDIR=./src/
dotnet publish %SRCDIR%OwinRequestScopeContext/OwinRequestScopeContext.csproj --framework netstandard2.0 --configuration Release --verbosity detailed /p:IsPublishing="true"
dotnet publish %SRCDIR%OwinRequestScopeContext/OwinRequestScopeContext.csproj --framework net462 --configuration Release --verbosity detailed /p:IsPublishing="true"
pause
