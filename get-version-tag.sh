#!/bin/bash
# Comment\ Uncomment this for trace output
# set -o xtrace

tag=$(git describe --tags --always --dirty --long)
commitCount=$(git rev-list --count HEAD)

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
	echo $version
fi