#!/bin/bash
# Comment\ Uncomment this for trace output
# set -o xtrace

tag=$(git describe --tags)
commitCount=$(git rev-list --count HEAD)
#If no tag has been added only the sha1 will be returned

#This will be the version in the format <major>.<minor>.<build number>.<revision>
major=$(echo $tag | cut -b '1-7' | cut -f1 -d'.')
minor=$(echo $tag | cut -b '1-7' | cut -f2 -d'.')
build=$(echo $tag | cut -b '1-7' | cut -f3 -d'.' )
revision=$(echo $tag | cut -b '1-7' | cut -f4 -d'.' )
# revision=${commitCount}
version="${major}"."${minor}"."${build}"."${revision}"
echo $version