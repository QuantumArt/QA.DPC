sha=$(git rev-parse --verify HEAD --short | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//')
echo '##vso[task.setvariable variable=GitVersion.Sha;]'$sha

tag=$(git describe --abbrev=0 --tags | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//')
echo '##vso[task.setvariable variable=GitVersion.Tag;]'$tag

cnt=$(git rev-list HEAD --count $tag..HEAD)
echo '##vso[task.setvariable variable=GitVersion.CommitsSinceTag;]'$cnt