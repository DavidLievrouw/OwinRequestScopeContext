@echo off
cls
SET SRCDIR=./src/
dotnet test %SRCDIR%OwinRequestScopeContext.Tests/OwinRequestScopeContext.Tests.csproj --verbosity detailed
pause
