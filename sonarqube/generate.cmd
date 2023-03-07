@ECHO OFF
SETLOCAL EnableDelayedExpansion

dotnet run --project GenerateSln\GenerateSln.csproj -- ..
