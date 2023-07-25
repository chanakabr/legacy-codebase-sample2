#!/bin/bash
# Comment\ Uncomment this for trace output
# set -o xtrace

startScanPath=$1
allProjFiles=$(find . -name *.csproj | xargs grep -rnwl -E "(netcoreapp[0-9]+\.[0-9]+)|netstandard[0-9]+\.[0-9]+")
tag=$(git describe --tags --always --dirty --long)

commitCount=$(git rev-list --count HEAD)
commiter=$(git config user.name)

echo "DllVersioning tag: $tag"
echo "DllVersioning commitCount: $commitCount"
echo "DllVersioning commiter: $commiter"
#If no tag has been added only the sha1 will be returned

#This will be the version in the format <major>.<minor>.<build number>.<revision>
major=$(echo $tag | cut -b '1-7' | cut -f1 -d'.')
minor=$(echo $tag | cut -b '1-7' | cut -f2 -d'.')
build=$(echo $tag | cut -b '1-7' | cut -f3 -d'.' )
revision=$(echo $tag | cut -b '1-7' | cut -f4 -d'.' )
# revision=${commitCount}
version="${major}"."${minor}"."${build}"."${revision}$2"
description="$(date +'%Y-%m-%d %H:%M:%S') \| Hostname:$(hostname) \| Published by:${commiter} \| Tag:${tag}"
echo "Identified Version: $version"
echo "Identified Description: $description"
echo 

for projFilePath in $allProjFiles; do
	echo "Patching project: $projFilePath"
	sed -i "s|\(<Version>\)[^<]*\(<\/Version>\)|\1$version\2|gi" $projFilePath
	sed -i "s|\(<AssemblyTitle>\)[^<]*\(<\/AssemblyTitle>\)|\1$description\2|gi" $projFilePath
done
