#!/bin/bash
set -ev

if [[ "$CFG" == "CORE" ]]; then
    dotnet build Tests -f netcoreapp1.0 -c Release
else
    msbuild /t:Rebuild /p:Configuration=Release /p:TargetFrameworks=net46 libsodium-net.sln
fi
