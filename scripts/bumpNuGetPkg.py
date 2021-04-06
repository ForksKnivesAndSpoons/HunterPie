import operator
import re
import sys

# get args
(_, previous, current, semver, *_) = sys.argv

version = re.match(r"[a-z]*([0-9]*\.[0-9]*\.[0-9]*).*", semver).group(1)

# subtract previous from current
p = map(int, previous.split('.'))
c = map(int, current.split('.'))
(bumped_M, bumped_m, bumped_p, *_) = map(operator.sub, c, p)

# bump as needed
(major, minor, patch, *_) = map(int, version.split('.'))
if bumped_M > 0 or bumped_m > 0:
    major+=1
    minor=0
    patch=0
elif bumped_p > 0:
    minor+=1
    patch=0
else:
    patch+=1

print(f'{major}.{minor}.{patch}')
