#!/bin/bash

if [[ $GITHUB_BASE_REF == "development" ]]
then
    suffix="-beta"
fi

echo "Suffix: $suffix"

version=$(cat $PROJECT/Properties/AssemblyInfo.cs | grep "^\[assembly: AssemblyVersion" | cut -d'"' -f2)
echo "Assembly Version: $version"
if [ "$BINARY" = "dll" ]
then
    version=$(echo $version | sed -r 's/(.*)\./\1-/')
fi
echo "Binary Version: $version"

PREVIOUS=$(git describe --match "$PREFIX$version-[0-9]*$suffix" --tags 2> /dev/null)
echo "Previous: $PREVIOUS"

if [ -z "$PREVIOUS" ]
then
    GIT_TAG="$PREFIX$version-0$suffix"
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
