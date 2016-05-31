copy "..\LICENSE.md" bin
copy README.md bin
"..\.nuget\NuGet.exe" pack logging.nuspec -BasePath bin