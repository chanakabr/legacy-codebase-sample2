#!/bin/bash
docker run --rm -e GITHUB_TOKEN=$GITHUB_TOKEN -v $PWD:/userdir -v $PWD/clients:/out  870777418594.dkr.ecr.us-west-2.amazonaws.com/master/ott-tool-codegen:build clients -s Phoenix -v 1.0.0
echo Finished generate service
