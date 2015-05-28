#!/bin/bash
#Git Post Merge Hook
#---------------------
#Gets the latest tag info from the git repo and updates the AssemblyInfo.cs file with it.
#This file needs to be place in the .git/hooks/ folder and only works when a git pull is
#made which contains changes in the remote repo.

#get all assembly files path in repository
assembly_files=$(git ls-files '*/AssemblyInfo.cs')

#get the latest tag info. The 'always' flag will give you a shortened SHA1 if no tag exists.
tag=$(git describe --always --dirty --long)
echo $tag

#If no tag has been added only the sha1 will be returned
if [[ $tag=="*.*" ]]
then
	IFS='.' read -ra TAG <<< "$tag"
	#echo "${TAG[0]}"
	#echo "${TAG[1]}"

	IFS='-' read -ra COMMITS <<< "${TAG[2]}"
	#echo "${COMMITS[0]}"
	#echo "${COMMITS[1]}"
	#echo "${COMMITS[2]}"

	#This will be the version in the format <major>.<minor>.<build number>.<revision>
	version="${TAG[0]}"."${TAG[1]}"."${COMMITS[0]}"."${COMMITS[1]}"
	echo $version
	
	#Update the AssemblyVersion and AssemblyFileVersion attribute with the 'version'
	for file in $assembly_files; do
	  sed -i.bak "s/\AssemblyVersion(\".*\")/AssemblyVersion(\"$version\")/g" $file 
	  sed -i.bak "s/\AssemblyFileVersion(\".*\")/AssemblyFileVersion(\"$version-"${COMMITS[2]}"-"${COMMITS[3]}"\")/g" $file 
	done
fi

#This swaps the AssemblyInformationalVersion attribute with the new git describe info
#sed -i.bak "s/\AssemblyInformationalVersion(\".*\")/AssemblyInformationalVersion(\"$tag\")/g" RestfulTVPApi/Properties/AssemblyInfo.cs

for file in $assembly_files; do
  sed -i.bak "s/\AssemblyInformationalVersion(\".*\")/AssemblyInformationalVersion(\"$tag\")/g" $file
  cmd //c "Icacls Properties /t /grant Everyone:(f)"
done

#cat $rootPath/Properties/AssemblyInfo.cs