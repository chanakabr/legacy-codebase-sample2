@ECHO OFF
SETLOCAL EnableDelayedExpansion

SET ARG=%1
SET ARG2=%2

SET UpdateTool=1
SET SCHEMA_BRANCH=master

IF DEFINED ARG (
    IF "%ARG%"=="/nu" (
        SET UpdateTool=0
    )
)

IF DEFINED ARG2 (
    SET SCHEMA_BRANCH=%ARG2%
)

:main
IF %UpdateTool% == 1 (
    ECHO Download latest ott-tool-codegen
    aws ecr get-login-password --region us-west-2 | docker login --username AWS --password-stdin 870777418594.dkr.ecr.us-west-2.amazonaws.com
    docker pull 870777418594.dkr.ecr.us-west-2.amazonaws.com/master/ott-tool-codegen:build
)

ECHO Generating Catalog events
docker run --rm -e GITHUB_TOKEN=${env.GITHUB_TOKEN} -v %cd%/Catalog:/userdir  870777418594.dkr.ecr.us-west-2.amazonaws.com/master/ott-tool-codegen:build service -srb %SCHEMA_BRANCH% -s phoenix --lang csharp
ECHO Generating Household events
docker run --rm -e GITHUB_TOKEN=${env.GITHUB_TOKEN} -v %cd%/Household:/userdir  870777418594.dkr.ecr.us-west-2.amazonaws.com/master/ott-tool-codegen:build service -srb %SCHEMA_BRANCH% -s phoenix --lang csharp

ECHO Generating ConditionalAccess events with ott-tool-codegen...
 docker run --rm -e GITHUB_TOKEN=${env.GITHUB_TOKEN} -v %cd%/ConditionalAccess:/userdir  870777418594.dkr.ecr.us-west-2.amazonaws.com/master/ott-tool-codegen:build service -srb %SCHEMA_BRANCH% -s phoenix --lang csharp

ECHO Generating Api events with ott-tool-codegen...
docker run --rm -e GITHUB_TOKEN=${env.GITHUB_TOKEN} -v %cd%/Api:/userdir  870777418594.dkr.ecr.us-west-2.amazonaws.com/master/ott-tool-codegen:build service -srb %SCHEMA_BRANCH% -s phoenix --lang csharp

ECHO Generating Pricing events with ott-tool-codegen...
docker run --rm -e GITHUB_TOKEN=${env.GITHUB_TOKEN} -v %cd%/Pricing:/userdir  870777418594.dkr.ecr.us-west-2.amazonaws.com/master/ott-tool-codegen:build service -srb %SCHEMA_BRANCH% -s phoenix --lang csharp

ECHO Generating ConditionalAccess events with ott-tool-codegen...
 docker run --rm -e GITHUB_TOKEN=${env.GITHUB_TOKEN} -v %cd%/ConditionalAccess:/userdir  870777418594.dkr.ecr.us-west-2.amazonaws.com/master/ott-tool-codegen:build service -srb %SCHEMA_BRANCH% -s phoenix --lang csharp

ECHO Generating Recording events with ott-tool-codegen...
 docker run --rm -e GITHUB_TOKEN=${env.GITHUB_TOKEN} -v %cd%/Recording:/userdir  870777418594.dkr.ecr.us-west-2.amazonaws.com/master/ott-tool-codegen:build service -srb %SCHEMA_BRANCH% -s phoenix --lang csharp

ECHO Generating Api events with ott-tool-codegen...
docker run --rm -e GITHUB_TOKEN=${env.GITHUB_TOKEN} -v %cd%/Api:/userdir  870777418594.dkr.ecr.us-west-2.amazonaws.com/master/ott-tool-codegen:build service -srb %SCHEMA_BRANCH% -s phoenix --lang csharp

ECHO Generating Pricing events with ott-tool-codegen...
docker run --rm -e GITHUB_TOKEN=${env.GITHUB_TOKEN} -v %cd%/Pricing:/userdir  870777418594.dkr.ecr.us-west-2.amazonaws.com/master/ott-tool-codegen:build service -srb %SCHEMA_BRANCH% -s phoenix --lang csharp


ECHO Generating events has been completed.
