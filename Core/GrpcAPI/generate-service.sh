#!/bin/bash
docker run --rm -e GITHUB_TOKEN=$GITHUB_TOKEN -v $PWD:/userdir  870777418594.dkr.ecr.us-west-2.amazonaws.com/master/ott-tool-codegen:build service -s Phoenix --lang csharp
echo Finished generate service
