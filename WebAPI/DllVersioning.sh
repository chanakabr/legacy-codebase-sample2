#!/bin/bash
#Git Post Merge Hook
#---------------------
#Gets the latest tag info from the git repo and updates the AssemblyInfo.cs file with it.
#This file needs to be place in the .git/hooks/ folder and only works when a git pull is
#made which contains changes in the remote repo.

#get all assembly files path in repository
echo $1
assembly_file=${1//[\\]//}Properties/AssemblyInfo.cs
echo $assembly_file 

#get the latest tag info. The 'always' flag will give you a shortened SHA1 if no tag exists.
tag=$(git describe --always --dirty --long)
echo $tag

#If no tag has been added only the sha1 will be returned
if [[ $tag == *.* ]]
then
	IFS='.' read -ra TAG <<< "$tag"
	echo "tag0-${TAG[0]}"
	echo "tag1-${TAG[1]}"
	echo "tag2-${TAG[2]}"
	
	echo "tag0-${TAG[0]}"
	echo "tag1-${TAG[1]}"
	echo "tag2-${TAG[2]}"
	
	echo $TAG 
	IFS='-' read -ra COMMITS <<< "${TAG[1]}"
	#echo "${COMMITS[0]}"
	#echo "${COMMITS[1]}"
	#echo "${COMMITS[2]}"

	#This will be the version in the format <major>.<minor>.<build number>.<revision>
	major=${TAG[0]}
	minor=${COMMITS[0]}
	build=${COMMITS[1]}
	revision=$(git rev-list --count --first-parent HEAD)
	version="${major}"."${minor}"."${build}".*""
	echo $version
	
	#Update the AssemblyVersion and AssemblyFileVersion attribute with the 'version'
	#for file in $assembly_files; do
	  sed -i.bak "s/\AssemblyVersion(\".*\")/AssemblyVersion(\"$version\")/g" $assembly_file 
	  sed -i.bak "s/\AssemblyFileVersion(\".*\")/AssemblyInformationalVersion(\"$(git describe --always --dirty --long)\")/g" $assembly_file
	  sed -i.bak "s/\AssemblyInformationalVersion(\".*\")/AssemblyInformationalVersion(\"$(git describe --always --dirty --long)\")/g" $assembly_file
	  sed -i.bak "s/\AssemblyDescription(\".*\")/AssemblyDescription(\"$(date +'%Y-%m-%d %H:%M:%S') | Hostname:$(hostname) | Published by:$(git config user.name)\")/g" $assembly_file
	#done
fi

#This swaps the AssemblyInformationalVersion attribute with the new git describe info
#sed -i.bak "s/\AssemblyInformationalVersion(\".*\")/AssemblyInformationalVersion(\"$tag\")/g" $assembly_file

#for file in $assembly_files; do
  sed -i.bak "s/\AssemblyInformationalVersion(\".*\")/AssemblyInformationalVersion(\"$tag\")/g" $assembly_file
  cmd //c "Icacls Properties /t /grant Everyone:(f)"
#done

#cat $rootPath/Properties/AssemblyInfo.cs