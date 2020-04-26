#!/bin/bash
# Comment\ Uncomment this for trace output
# set -o xtrace

startScanPath=$1
allProjFiles=$(grep --include=\*.csproj -rnwl -E "(netcoreapp[0-9]+\.[0-9]+)|netstandard[0-9]+\.[0-9]+" ${startScanPath})
long_tag=$(git describe --tags --always --dirty --long)
tag=$(git describe --tags)

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
	if [[ ${COMMITS[2]} ]];then 
		fp=${COMMITS[2]}
		version="${major}"."${minor}"."${build}"."${fp}"\|"${revision}"
	else
		version="${major}"."${minor}"."${build}"."${revision}"
	fi
	description="$(date +'%Y-%m-%d %H:%M:%S') \| Hostname:$(hostname) \| Published by:${commiter} \| Tag:${long_tag}"
	echo "Identified Version: $version"
	echo "Identified Description: $description"
	echo 


	for projFilePath in $allProjFiles; do
		echo "Patching project: $projFilePath"
		sed -i "s|\(<Version>\)[^<]*\(<\/Version>\)|\1$version\2|gi" $projFilePath
		sed -i "s|\(<AssemblyTitle>\)[^<]*\(<\/AssemblyTitle>\)|\1$description\2|gi" $projFilePath
	done

fi