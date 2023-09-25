# 1. AWS Access Key ID and AWS Secret Access Key must be set before this script is executed:
# aws configure
#
# 2. This script expects that a valid GitHub token has been set as environment variable GITHUB_TOKEN:
# export GITHUB_TOKEN=your-GitHub-token
#
# 3. SCHEMA_BRANCH can be used to specify a branch in ott-lib-schema-registry repository
# based on which classes are generated. master branch is used by default.

AWS_REGION="us-west-2"
AWS_HOST="870777418594.dkr.ecr.us-west-2.amazonaws.com"
BASH_DIR="$(cd "$(dirname "$0")"; pwd -P)"
SCHEMA_BRANCH=master

if [ -z $GITHUB_TOKEN ]
then
    echo -n GITHUB_TOKEN environment variable must be set. Please specify your GitHub token to proceed: 
    read -s GITHUB_TOKEN
    echo
fi

if [ -z $1 ] || [ $1 != "/nu" ]
then
	echo AWS login...
	aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $AWS_HOST

	echo Downloading latest ott-tool-codegen...
	docker pull $AWS_HOST/master/ott-tool-codegen:build
fi

echo Generating Bookmark events
docker run --rm -e GITHUB_TOKEN=$GITHUB_TOKEN -v $BASH_DIR/Bookmark:/userdir $AWS_HOST/master/ott-tool-codegen:build service -srb $SCHEMA_BRANCH -s phoenix --lang csharp

docker run --rm -e GITHUB_TOKEN=$GITHUB_TOKEN -v $BASH_DIR/Catalog:/userdir $AWS_HOST/master/ott-tool-codegen:build service -srb $SCHEMA_BRANCH -s phoenix --lang csharp
echo Generating Household events
docker run --rm -e GITHUB_TOKEN=$GITHUB_TOKEN -v $BASH_DIR/Household:/userdir $AWS_HOST/master/ott-tool-codegen:build service -srb $SCHEMA_BRANCH -s phoenix --lang csharp

echo Generating ConditionalAccess events with ott-tool-codegen...
docker run --rm -e GITHUB_TOKEN=$GITHUB_TOKEN -v $BASH_DIR/ConditionalAccess:/userdir $AWS_HOST/master/ott-tool-codegen:build service -srb $SCHEMA_BRANCH -s phoenix --lang csharp

echo Generating Api events with ott-tool-codegen...
docker run --rm -e GITHUB_TOKEN=$GITHUB_TOKEN -v $BASH_DIR/Api:/userdir $AWS_HOST/master/ott-tool-codegen:build service -srb $SCHEMA_BRANCH -s phoenix --lang csharp

echo Generating Pricing events with ott-tool-codegen...
docker run --rm -e GITHUB_TOKEN=$GITHUB_TOKEN -v $BASH_DIR/Pricing:/userdir $AWS_HOST/master/ott-tool-codegen:build service -srb $SCHEMA_BRANCH -s phoenix --lang csharp

echo Generating ConditionalAccess events with ott-tool-codegen...
docker run --rm -e GITHUB_TOKEN=$GITHUB_TOKEN -v $BASH_DIR/ConditionalAccess:/userdir $AWS_HOST/master/ott-tool-codegen:build service -srb $SCHEMA_BRANCH -s phoenix --lang csharp

echo Generating Api events with ott-tool-codegen...
docker run --rm -e GITHUB_TOKEN=$GITHUB_TOKEN -v $BASH_DIR/Api:/userdir $AWS_HOST/master/ott-tool-codegen:build service -srb $SCHEMA_BRANCH -s phoenix --lang csharp

echo Generating Pricing events with ott-tool-codegen...
docker run --rm -e GITHUB_TOKEN=$GITHUB_TOKEN -v $BASH_DIR/Pricing:/userdir $AWS_HOST/master/ott-tool-codegen:build service -srb $SCHEMA_BRANCH -s phoenix --lang csharp

echo Generating Recording events with ott-tool-codegen...
docker run --rm -e GITHUB_TOKEN=$GITHUB_TOKEN -v $BASH_DIR/Recording:/userdir $AWS_HOST/master/ott-tool-codegen:build service -srb $SCHEMA_BRANCH -s phoenix --lang csharp


echo Generating events has been completed.