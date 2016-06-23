@setlocal 
@set local=%~dp0
@ECHO off

@REM Get path to MSBuild Binaries
if exist "%ProgramFiles%\MSBuild\14.0\bin" SET MSBUILDEXEDIR=%ProgramFiles%\MSBuild\14.0\bin
if exist "%ProgramFiles(x86)%\MSBuild\14.0\bin" SET MSBUILDEXEDIR=%ProgramFiles(x86)%\MSBuild\14.0\bin

@REM Can't multi-block if statement when check condition contains '(' and ')' char, so do as single line checks
if NOT "%MSBUILDEXEDIR%" == "" SET MSBUILDEXE=%MSBUILDEXEDIR%\MSBuild.exe
if "%MSBUILDEXEDIR%" == "" GOTO :MsBuildNotFound
if NOT "%MSBUILDEXEDIR%" == "" GOTO :MsBuildFound


:MsBuildFound
@ECHO MsBuild Location = %MSBUILDEXE%
@goto build

:build
"%MSBUILDEXE%" "%local%Jusfr.Persistent.sln" /t:Rebuild /P:Configuration=Release
@goto copy


:copy
robocopy "%local%src\Jusfr.Persistent.NET40\bin\Release" "%local%release" /e
robocopy "%local%src\Jusfr.Persistent.NHibernate\bin\Release" "%local%release" /e
robocopy "%local%src\Jusfr.Persistent.Mongo\bin\Release" "%local%release" /e
@goto pack

:pack
%local%\.nuget\NuGet pack %local%\src\Jusfr.Persistent\Jusfr.Persistent.nuspec  -OutputDirectory %local%\release
%local%\.nuget\NuGet pack %local%\src\Jusfr.Persistent.Mongo\Jusfr.Persistent.Mongo.nuspec  -OutputDirectory %local%\release
@goto end


:end
@pushd %local%
@pause

:MsBuildNotFound
@ECHO Could not found msbuild