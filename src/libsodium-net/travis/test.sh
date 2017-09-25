#!/bin/bash
set -ev

if [[ "$CFG" == "CORE" ]]; then
  dotnet test Tests/Tests.csproj -f netcoreapp1.0 -c Release
else
  mono ./testrunner/NUnit.ConsoleRunner.3.6.1/tools/nunit3-console.exe ./Tests/bin/Release/net46/Tests.dll
fi
