
param([String]$Project=30)


function Replace-In-File{
    [CmdletBinding()]
    Param(
        [Parameter()]
        [String] 
        $file,

        [Parameter()]
        [String] 
        $search,
        
        [Parameter()]
        [String] 
        $replacement
    )
	
	$content = Get-Content -Path $file | % { $_ -replace $search, $replacement }
	Set-Content -Path $file -Value $content
}

Write-Host "Versioning project folder $Project";
$assemblyFile = "$Project/Properties/AssemblyInfo.cs"

if(![System.IO.File]::Exists($assemblyFile)) {
	Write-Host "Assembly file [$assemblyFile] not found"
	exit -1
}

#get the latest tag info. The 'always' flag will give you a shortened SHA1 if no tag exists.
$tag = $(git describe --always --dirty --long)
Write-Host "Git tag: $tag"

$tagParts = $tag -split '\.'

#If no tag has been added only the sha1 will be returned
if($tagParts.Length -ge 2) {
	$commitParts = $tag -split '-'

	$major = $tagParts[0]
	$minor = $commitParts[0]
	$build = $commitParts[1]
	$revision = $(git rev-list --count --first-parent HEAD)
	$version = "$major.$minor.$build.$revision"

	$date = $(date +'%Y-%m-%d %H:%M:%S')
	$hostname = $(hostname)
	$committer = $(git config user.name)
	$description = "$date | Hostname:$hostname | Published by:$committer"

	Write-Host "Version: $version, $description"
	
	#Update the AssemblyVersion and AssemblyFileVersion attribute with the 'version'	
	Replace-In-File -file $assemblyFile -search 'AssemblyVersion\("[^"]+"\)' -replacement "AssemblyVersion(""$version"")"
	Replace-In-File -file $assemblyFile -search 'AssemblyFileVersion\("[^"]+"\)' -replacement "AssemblyFileVersion(""$tag"")"
	Replace-In-File -file $assemblyFile -search 'AssemblyInformationalVersion\("[^"]+"\)' -replacement "AssemblyInformationalVersion(""$tag"")"
	Replace-In-File -file $assemblyFile -search 'AssemblyDescription\("[^"]+"\)' -replacement "AssemblyDescription(""$description"")"
}
