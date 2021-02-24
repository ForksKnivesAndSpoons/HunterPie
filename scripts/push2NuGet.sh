#!/bin/bash

version=$(echo $GIT_TAG | grep -o '[0-9].*')
suffix=$(echo $version | grep -o '\-beta' | tr '-' '.')
version=${version%-beta}

echo "suffix: $suffix"
echo "version: $version"

nuget sources add -name "Github" \
      -Source https://nuget.pkg.github.com/$GITHUB_REPOSITORY_OWNER/index.json \
      -Username $GITHUB_REPOSITORY_OWNER \
      -Password $GITHUB_TOKEN

echo "packing $PROJECT$suffix"
nuget pack $PROJECT -Prop Configuration=Release -Properties "repo=$GITHUB_REPOSITORY;nugetVersion=$version;id=$PROJECT$suffix"

echo "pushing $PROJECT$suffix.$version.nupkg"
nuget push $PROJECT$suffix.$version.nupkg -Source "Github" -ApiKey $GITHUB_TOKEN -SkipDuplicate;
