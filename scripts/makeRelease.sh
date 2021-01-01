#!/bin/bash

suffix=
prerelease=
branch=${GITHUB_REF##*/}
echo $branch

if [[ $branch == "development" ]]
then
    suffix="-beta"
    prerelease="-p"
fi

echo "Suffix: $suffix"
echo "Prerelease flag: $prerelease"

version=$(cat HunterPie/Properties/AssemblyInfo.cs | grep "^\[assembly: AssemblyVersion" | cut -d'"' -f2)
echo "Version: $version"

previous=$(git describe --match "v$version-[0-9]*$suffix" --tags 2> /dev/null)
echo "Previous: $previous"
if [ -z "$previous" ]
then
    GIT_TAG="v$version-0$suffix"
else
    bump=$(echo ${previous%-beta} | rev | cut -d'-' -f1)
    ((bump++))
    GIT_TAG="v$version-$bump$suffix"
fi
echo "GIT_TAG: $GIT_TAG"

gh release create $GIT_TAG --title "Release $GIT_TAG" --notes-file notes.md $prerelease bin/*
