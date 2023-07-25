#!/bin/bash
# Comment\ Uncomment this for trace output
# set -o xtrace

tag=$(git describe --tags)
commitCount=$(git rev-list --count HEAD)

echo "get-version-tag tag: $tag"
echo "get-version-tag commitCount: $commitCount"
#If no tag has been added only the sha1 will be returned

#This will be the version in the format <major>.<minor>.<build number>.<revision>
major=$(echo $tag | cut -f1 -d'.')
minor=$(echo $tag | cut -f2 -d'.')
build=$(echo $tag | cut -f3 -d'.' )
revision=$(echo $tag | cut -f4 -d'.' )
# revision=${commitCount}
version="${major}"."${minor}"."${build}"."${revision}"
echo "get-version-tag version: $version"
echo $version