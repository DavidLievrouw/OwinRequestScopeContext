@ECHO OFF
SETLOCAL enabledelayedexpansion

SET candidatesCount=3
SET candidates[0]=%PROGRAMFILES(X86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe
SET candidates[1]=%PROGRAMFILES(X86)%\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe
SET candidates[2]=%PROGRAMFILES(X86)%\MSBuild\14.0\Bin\MSBuild.exe

FOR /F "tokens=2" %%i IN ('date /t') DO SET currentDate=%%i
SET currentTime=%time%

SET /a "limit=(%candidatesCount%-1)"
FOR /l %%n IN (0,1,%limit%) DO (
  ECHO %currentDate% %currentTime% Checking if file !candidates[%%n]! exists...
  IF EXIST !candidates[%%n]! (
    IF NOT DEFINED MSBUILD (
      SET MSBUILD=!candidates[%%n]!
	)
  )
)
IF NOT DEFINED MSBUILD (
  SET MSBUILD=!candidates[2]!
)
	
ECHO %currentDate% %currentTime% MSBuild path resolved to "%MSBUILD%".
ECHO %MSBUILD%

ENDLOCAL