#!/bin/bash
# Comment\ Uncomment this for trace output
# set -o xtrace

startScanPath=$1
allProjFiles=$( grep --include=\*.csproj -rnwl ${startScanPath} -e "netstandard2.0")
tag=$(git describe --always --dirty --long)

echo "VERSION_TAG: $VERSION_TAG"
commitCount=$(git rev-list --count HEAD)
commiter=$(git config user.name)

echo "tag: $tag"
echo "commit count: $commitCount"
#If no tag has been added only the sha1 will be returned
if [[ $tag == *.* ]]
then
	IFS='.' read -ra TAG <<< "$tag"
	IFS='-' read -ra COMMITS <<< "${TAG[1]}"

	#This will be the version in the format <major>.<minor>.<build number>.<revision>
	major=${TAG[0]}
	minor=${COMMITS[0]}
	build=${COMMITS[1]}
    revision=${commitCount}
	version="${major}"."${minor}"."${build}"."${revision}"
    description="$(date +'%Y-%m-%d %H:%M:%S') \| Hostname:$(hostname) \| Published by:${commiter} \| Tag:${tag}"
	echo "Identified Version: $version"
    echo "Identified Description: $description"
	echo 


    for projFilePath in $allProjFiles; do
        echo "Patching project: $projFilePath"
        sed -i "s|\(<Version>\)[^<]*\(<\/Version>\)|\1$version\2|gi" $projFilePath
        sed -i "s|\(<Description>\)[^<]*\(<\/Description>\)|\1$description\2|gi" $projFilePath
    done

fi