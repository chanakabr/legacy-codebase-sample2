# Regenerate `sln`s
run `generate.cmd`

# Motivation
- need to scan all projects with [sonarqube](https://kaltura.atlassian.net/wiki/spaces/~240099400/pages/2236350468/Sonarqube)

# Modifications
Separate solution files were auto-generated `sonarqube/netcore/ott-be-net-core.sln` and `sonarqube/netframework/ott-be-net-framework.sln`. Part of the projects were excluded as they don't compile and most probably not used. Look at `GenerateSln.cs` for details.

# Troubleshooting
You'll have compilation errors during `msbuild` of `ott-be-net-framework.sln`. how to fix:
- `Phoenix.Legacy.csproj` and `TVPApi.Legacy.csproj`, in the end of the file remove/comment section `Target` with `EnsureNuGetPackageBuildImports`
- `FileUploadHandler.csproj` remove/comment `ItemGroup` -> `Analyzer` -> `AWSSDK.S3.CodeAnalysis`
- `packages` folder should be copied to `tvpapi` folder

