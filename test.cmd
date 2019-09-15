@echo off
cls
SET SRCDIR=./src/
dotnet test %SRCDIR%OwinRequestScopeContext.Tests/OwinRequestScopeContext.Tests.csproj --framework net461
dotnet test %SRCDIR%OwinRequestScopeContext.Tests/OwinRequestScopeContext.Tests.csproj --framework netcoreapp2.2
pause
