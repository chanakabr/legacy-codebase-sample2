param([String]$BasePath)

function Get-Version-Git {
    #get the latest tag info. The 'always' flag will give you a shortened SHA1 if no tag exists.
    $tag = $(git describe --tag --always --abbrev=0)
    $commitCount = $(git rev-list --count HEAD)
   
    $tagParts = $tag -split '\.'
   
    #If no tag has been added only the sha1 will be returned
    if ($tagParts.Length -ge 2) {
        $commitParts = $tagParts[1] -split '-'
   
        $major = $tagParts[0]
        $minor = $commitParts[0]
        $build = $commitParts[1]
        # Write-Host "Major: $major"
        # Write-Host "Minor: $minor"
        # Write-Host "Build: $build"
   
        #$revision = $(git rev-list --count --first-parent HEAD)
        # $version = "$major.$minor.$build.$commitCount"
        $version = $tag
        return $version
    }
}

 
function Get-Description {
    $tag = $(git describe --tag --always --abbrev=0)
    $date = get-date -format "yyy-mm-dd HH:MM:ss"
    $hostname = $(hostname)
    $committer = $(git config user.name)
    $description = "$date | Hostname:$hostname | Published by:$committer | Tag:$tag"
    return $description
}

function Replace-In-File {
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

function Is-NetCore{
    [CmdletBinding()]
    Param(
        [Parameter()]
        [String] 
        $ProjectPath
    )

    $isNetCoreProject = 0

    $assemblyFile = "$ProjectPath\Properties\AssemblyInfo.cs"
    Write-Host "assemblyFile" $assemblyFile
    if (![System.IO.File]::Exists($assemblyFile)) {
        $isNetCoreProject = 1
    }

    return $isNetCoreProject
}

function Version-Patch-Project { 
    [CmdletBinding()]
    Param(
        [Parameter()]
        [String] 
        $ProjectFilePath,

        [Parameter()]
        [String] 
        $Version
    )

    $ProjectPath = Split-Path -Path $ProjectFilePath
    $isNetCoreProject = Is-NetCore -ProjectPath $ProjectPath
    
    if ($isNetCoreProject -eq 1) {
        Write-Host "Project Identified as .NetCore, patching .csproj file"

        $projXml = [xml](Get-Content $ProjectFilePath)
        $firstPropertyGroup = $projXml.SelectNodes("//PropertyGroup")[0]
        $versionNode = $projXml.SelectNodes("//Version")[0]
        $descriptionNode = $projXml.SelectNodes("//Description")[0]

        Write-Host "firstPropertyGroup: $firstPropertyGroup"
        Write-Host "versionNode: $versionNode"
        Write-Host "descriptionNode: $descriptionNode"

        if (!$versionNode) {
            Write-Host "No <Version> node found, creating new one. setting value to $version"
            $newVersionNode = $projXml.CreateElement("Version")
            $newVersionNode.InnerText = "$version"
            $firstPropertyGroup.AppendChild($newVersionNode)
        }
        else {
            Write-Host "<Version> node set to: $version"
            $versionNode.InnerText = "$version"
        }

        
        if (!$descriptionNode) {
            Write-Host "No <Description> node found, creating new one. setting value to: $description"
            $newDescriptionNode = $projXml.CreateElement("Description")
            $newDescriptionNode.InnerText = "$description"
            $firstPropertyGroup.AppendChild($newDescriptionNode)
        }
        else {
            Write-Host "<Description> node set to: $description"
            $descriptionNode.InnerText = $description
        }

        $projXml.Save($ProjectFilePath)
    }
    else {
        $assemblyFile = "$ProjectPath\Properties\AssemblyInfo.cs"
        Write-Host "Project Identified as .NetFramework, patching $assemblyFile"

        Replace-In-File -file $assemblyFile -search 'AssemblyVersion\("[^"]+"\)' -replacement "AssemblyVersion(""$version"")"
        Replace-In-File -file $assemblyFile -search 'AssemblyFileVersion\("[^"]+"\)' -replacement "AssemblyFileVersion(""$version"")"
        Replace-In-File -file $assemblyFile -search 'AssemblyInformationalVersion\("[^"]+"\)' -replacement "AssemblyInformationalVersion(""$version"")"
        Replace-In-File -file $assemblyFile -search 'AssemblyDescription\("[^"]+"\)' -replacement "AssemblyDescription(""$description"")"
    }
}



if ([string]::IsNullOrEmpty($BasePath)) {
    $ProjectFilesList = get-childitem -File -Depth 5 -Filter "*.csproj" | Select-Object *
}
else {
    $ProjectFilesList = get-childitem -File -Depth 5 -Filter "*.csproj" -Path $BasePath | Select-Object *
}



$version = Get-Version-Git
$description = Get-Description
Write-Host "Version: $version"
Write-Host "Description:  $description"

foreach ($ProjectFile in $ProjectFilesList) {
    Write-Host "Patching Project:" $ProjectFile.Name "Version:" $version
    Version-Patch-Project -ProjectFilePath $ProjectFile.FullName -Version $version
    Write-Host "Patching Done"
    Write-Host ""
}

exit 0