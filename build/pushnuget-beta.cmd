@echo off
cls
SET DIR=%~dp0%
IF NOT EXIST "%DIR%\log" MKDIR "%DIR%\log"
FOR /f "delims=" %%i IN ('CALL %DIR%\ResolveMSBuild.cmd') DO (
  SET MSBUILD=%%i
  ECHO %%i >> %DIR%/log/ResolveMSBuild.log
)
"%MSBUILD%" /m /v:n "%DIR%OwinRequestScopeContext.proj" /p:IsPrerelease="true";PrereleaseName="beta" /target:PushNuGet /logger:FileLogger,Microsoft.Build.Engine;LogFile=%DIR%log/pushnuget.log
pause