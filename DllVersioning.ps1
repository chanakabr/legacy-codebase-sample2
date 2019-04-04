
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

$isNetCoreProject = 0


$assemblyFile = "$Project/Properties/AssemblyInfo.cs"
$projFile =  Get-ChildItem -Path *.csproj | Select-Object -last 1
Write-Host "Versioning project folder $Project";
Write-Host "AssemblyFile $assemblyFile";
Write-Host "ProjFile $projFile";

if(![System.IO.File]::Exists($assemblyFile)) {
	Write-Host "Assembly file [$assemblyFile] not found"
	Write-Host "Assuming project is .net core"
    $isNetCoreProject = 1
    
}

#get the latest tag info. The 'always' flag will give you a shortened SHA1 if no tag exists.
$tag = $(git describe --always --dirty --long)
$commitCount = $(git rev-list --count HEAD)
Write-Host "Git tag: $tag"
Write-Host "Git commitCount: $commitCount"

$tagParts = $tag -split '\.'

#If no tag has been added only the sha1 will be returned
if($tagParts.Length -ge 2) {
	$commitParts = $tagParts[1] -split '-'

	$major = $tagParts[0]
	$minor = $commitParts[0]
	$build = $commitParts[1]
	# Write-Host "Major: $major"
	# Write-Host "Minor: $minor"
	# Write-Host "Build: $build"

	#$revision = $(git rev-list --count --first-parent HEAD)
	$version = "$major.$minor.$build.$commitCount"

	$date = get-date -format "yyy-mm-dd HH:MM:ss"
	$hostname = $(hostname)
	$committer = $(git config user.name)
	$description = "$date | Hostname:$hostname | Published by:$committer | Tag:$tag"

	Write-Host "Version: $version"
    Write-Host "Description:  $description"
	
	#Update the AssemblyVersion and AssemblyFileVersion attribute with the 'version'	
    if ($isNetCoreProject -eq 1) {
        $projXml = [xml](Get-Content $projFile)
        $firstPropertyGroup = $projXml.SelectNodes("//PropertyGroup")[0]
        $versionNode = $projXml.SelectNodes("//Version")[0]
        $descriptionNode = $projXml.SelectNodes("//Description")[0]

        Write-Host "firstPropertyGroup: $firstPropertyGroup"
        Write-Host "versionNode: $versionNode"
        Write-Host "descriptionNode: $descriptionNode"

        if (!$versionNode){
            Write-Host "No <Version> node found, creating new one. setting value to $version"
            $newVersionNode = $projXml.CreateElement("Version")
            $newVersionNode.InnerText = "$version"
            $firstPropertyGroup.AppendChild($newVersionNode)
        }
        else{
            Write-Host "<Version> node set to: $version"
            $versionNode.InnerText = "$version"
        }

        
        if (!$descriptionNode){
            Write-Host "No <Description> node found, creating new one. setting value to: $description"
            $newDescriptionNode = $projXml.CreateElement("Description")
            $newDescriptionNode.InnerText = "$description"
            $firstPropertyGroup.AppendChild($newDescriptionNode)
        }
        else{
            Write-Host "<Description> node set to: $description"
            $descriptionNode.InnerText = $description
        }

        $projXml.Save($projFile)
    }
    else {
        Replace-In-File -file $assemblyFile -search 'AssemblyVersion\("[^"]+"\)' -replacement "AssemblyVersion(""$version"")"
        Replace-In-File -file $assemblyFile -search 'AssemblyFileVersion\("[^"]+"\)' -replacement "AssemblyFileVersion(""$version"")"
        Replace-In-File -file $assemblyFile -search 'AssemblyInformationalVersion\("[^"]+"\)' -replacement "AssemblyInformationalVersion(""$version"")"
        Replace-In-File -file $assemblyFile -search 'AssemblyDescription\("[^"]+"\)' -replacement "AssemblyDescription(""$description"")"
    }
}
