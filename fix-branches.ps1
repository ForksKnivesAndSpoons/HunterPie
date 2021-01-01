$current=$(git branch --show-current)

git checkout master
$parts=$(git describe --first-parent --abbrev=0) -split '-'
$parts[-1] = ($parts[-1] -as [int]) + 1
$tag=$($parts -join '-')

git fetch upstream master
git reset --hard FETCH_HEAD
git cherry-pick ci~ ci
git push --force

git checkout ci
git reset --hard master
git push --force

git checkout dev
git fetch upstream development
git reset --hard FETCH_HEAD
git push --force

git checkout development
git reset --hard dev
git merge master
git push --force

git checkout $current
git tag -a $tag master -m "$tag"
git push --tag
