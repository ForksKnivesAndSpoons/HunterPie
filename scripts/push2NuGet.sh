#!/bin/bash

version=$(echo $GIT_TAG | grep -o '[0-9].*')
prefix=$(echo $GIT_TAG | grep -o '^[a-z]*')
suffix=$(echo $version | grep -o '\-beta' | tr '-' '.')
beta=$(echo $version | grep -o '\-beta')
version=${version%%-*}

previous=$(echo $PREVIOUS_TAG | grep -o '[0-9].*')
previous=${previous%%-*}


if [[ $beta == "-beta" ]]
then
    excludes=""
else
    excludes='*-beta'
fi

semver=$(git describe --match "$prefix[0-9]*.[0-9]*.[0-9]*$beta" --tags --abbrev=0 --exclude "*-[0-9]*" --exclude "$excludes" 2> /dev/null)

echo "prefix: $prefix"
echo "suffix: $suffix"
echo "version: $version"

echo "bumpNuGetPkg.py $previous $version $semver"
semver=$(python3 scripts/bumpNuGetPkg.py $previous $version $semver)
echo "bumped to $semver"

nuget sources add -name "Github" \
      -Source https://nuget.pkg.github.com/$GITHUB_REPOSITORY_OWNER/index.json \
      -Username $GITHUB_REPOSITORY_OWNER \
      -Password $GITHUB_TOKEN

echo "packing $PROJECT$suffix"
nuget pack $PROJECT -Prop Configuration=Release -Properties "repo=$GITHUB_REPOSITORY;nugetVersion=$semver$beta;id=$PROJECT$suffix"

echo "pushing $PROJECT$suffix.$semver$beta.nupkg"
nuget push $PROJECT$suffix.$semver$beta.nupkg -Source "Github" -ApiKey $GITHUB_TOKEN -SkipDuplicate;


git config user.name "Github Actions"
git config user.email action@github.com
git tag -a $prefix$semver$beta -m "$prefix$semver$beta"
git push --tags
