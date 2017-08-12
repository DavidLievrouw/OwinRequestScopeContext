@echo off
cls
SET DIR=%~dp0%
IF NOT EXIST "%DIR%\log" MKDIR "%DIR%\log"
FOR /f "delims=" %%i IN ('CALL %DIR%\ResolveMSBuild.cmd') DO (
  SET MSBUILD=%%i
  ECHO %%i >> %DIR%/log/ResolveMSBuild.log
)
"%MSBUILD%" /m /v:n "%DIR%/restorepackages.proj" /target:RestorePackages /logger:FileLogger,Microsoft.Build.Engine;LogFile=%DIR%log/restorepackages.log
pause