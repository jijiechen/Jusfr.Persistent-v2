@setlocal 
@set local=%~dp0
@pushd %WINDIR%\Microsoft.NET\Framework\v4.0.30319\
@goto build

:build
msbuild "%local%src\Jusfr.Persistent.sln" /t:Rebuild /P:Configuration=Release
@goto pack

:pack
%local%\.nuget\NuGet pack %local%\src\Jusfr.Persistent.Mongo\Jusfr.Persistent.Mongo.nuspec  -OutputDirectory %local%\release
@goto copy

:copy
robocopy "%local%src\Jusfr.Persistent\bin\Release" "%local%release" /e
robocopy "%local%src\Jusfr.Persistent.NH\bin\Release" "%local%release" /e
robocopy "%local%src\Jusfr.Persistent.Mongo\bin\Release" "%local%release" /e
@goto end

:end
@pushd %local%
@pause