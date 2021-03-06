#!/bin/bash

suffix=
excludes=
branch=${GITHUB_REF##*/}
echo $branch

if [[ $branch == "development" ]]
then
    suffix="-beta"
else
    excludes='*-beta'
fi

echo "Suffix: $suffix"
echo "Excludes: $excludes"

version=$(cat $PROJECT/Properties/AssemblyInfo.cs | grep "^\[assembly: AssemblyVersion" | cut -d'"' -f2)
echo "Assembly Version: $version"

PREVIOUS=$(git describe --match "$PREFIX$version-[0-9]*$suffix" --tags --abbrev=0 --exclude "$excludes" 2> /dev/null)
echo "Previous: $PREVIOUS"

if [ -z "$PREVIOUS" ]
then
    GIT_TAG="$PREFIX$version-0$suffix"
    PREVIOUS=$(git describe --match "$PREFIX[0-9]*.[0-9]*.[0-9]*.[0-9]*-[0-9]*$suffix" --tags --abbrev=0 --exclude "$excludes" 2> /dev/null)
else
    if [ -n "$(git diff --name-only $PREVIOUS..HEAD -- $PROJECT/)" ]
    then
    bump=$(echo ${PREVIOUS%-beta} | rev | cut -d'-' -f1)
    ((bump++))
    GIT_TAG="$PREFIX$version-$bump$suffix"
    fi
fi

echo "GIT_TAG=$GIT_TAG"
echo "$GIT_TAG" > version
echo "GIT_TAG=$GIT_TAG" >> $GITHUB_ENV
echo "PREVIOUS_TAG=$PREVIOUS" >> $GITHUB_ENV
