ARG DOTNET_FRAMEWORK_TAG=4.7.2-sdk-windowsservercore-ltsc2019
ARG IIS_TAG=windowsservercore-ltsc2019
ARG BUILD_TAG=latest
FROM microsoft/dotnet-framework:${DOTNET_FRAMEWORK_TAG} AS dotnetframework
FROM 870777418594.dkr.ecr.eu-west-1.amazonaws.com/core:${BUILD_TAG} AS builder
SHELL ["powershell"]

ARG BRANCH=master
ARG BITBUCKET_TOKEN

ADD . tvmapps

RUN nuget restore tvmapps/tvmapps.sln

RUN msbuild /p:Configuration=Release tvmapps/tvmapps.sln
RUN msbuild /t:WebPublish /p:Configuration=Release /p:DeployOnBuild=True /p:WebPublishMethod=FileSystem /p:PublishUrl=C:/tvmapps tvmapps/Web` Sites/TVM/website.publishproj