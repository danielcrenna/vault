copy "..\LICENSE.md" bin
copy README.md bin
"..\.nuget\NuGet.exe" pack MiniRack.nuspec -BasePath bin