#!/bin/bash
docker run --rm -e GITHUB_TOKEN=$GITHUB_TOKEN -v $PWD/ssoAdapter:/userdir -v $PWD/ssoAdapter/clients:/out  870777418594.dkr.ecr.us-west-2.amazonaws.com/master/ott-tool-codegen:build clients -s SsoAdapter -v 1.0.0
echo Finished generate service
