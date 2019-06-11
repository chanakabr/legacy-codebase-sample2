VERSION_TAG=$(git describe --always --dirty --long --tags)
VERSION_TAG=${VERSION_TAG//-/.}
echo $VERSION_TAG | cut -f1-3 -d"."