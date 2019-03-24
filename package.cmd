@echo OFF
if "%1" == "" (
  set /p version="Enter semver: "
) else (
set version=%1
)
cd src\ResourceEmbedder.MsBuild
dotnet pack -c Release /p:Version=%version%

cd ..\ResourceEmbedder.Core
dotnet pack -c Release /p:Version=%version%

cd..\..