#!/bin/bash
set -ev

if [[ "$CFG" == "CORE" ]]; then
  dotnet restore libsodium-net.sln
else
  curl -O https://dist.nuget.org/win-x86-commandline/v4.1.0/nuget.exe
  lnuget () {
	mono --runtime=v4.0 ./nuget.exe "$@"
  }
  
  lnuget restore libsodium-net.sln
  lnuget install NUnit.ConsoleRunner -Version 3.6.1 -OutputDirectory testrunner
fi
