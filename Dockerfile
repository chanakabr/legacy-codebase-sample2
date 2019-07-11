﻿ARG DOTNET_FRAMEWORK_TAG=4.7.2-sdk-windowsservercore-ltsc2019
FROM microsoft/dotnet-framework:${DOTNET_FRAMEWORK_TAG} AS builder
SHELL ["powershell"]

ARG BRANCH=master
ARG BITBUCKET_TOKEN
ARG API_VERSION

ARG TCM_URL=http://tcm:8080/
ARG TCM_APP=OTT_API_${API_VERSION}
ARG TCM_SECTION=base
ARG TCM_APP_ID=5bf8cf60
ARG TCM_APP_SECRET=5aaa99331c18f6bad4adeef93ab770c2

ENV TCM_URL ${TCM_URL}
ENV TCM_APP ${TCM_APP}
ENV TCM_SECTION ${TCM_SECTION}
ENV TCM_APP_ID ${TCM_APP_ID}
ENV TCM_APP_SECRET ${TCM_APP_SECRET}

ADD https://${BITBUCKET_TOKEN}@bitbucket.org/tvinci_dev/tvmcore/get/${BRANCH}.zip TvmCore.zip
ADD https://${BITBUCKET_TOKEN}@bitbucket.org/tvinci_dev/CDNTokenizers/get/${BRANCH}.zip CDNTokenizers.zip

RUN Expand-Archive TvmCore.zip -DestinationPath tmp; mv tmp/$((Get-ChildItem tmp | Select-Object -First 1).Name) TvmCore; rm tmp
RUN Expand-Archive CDNTokenizers.zip -DestinationPath tmp; mv tmp/$((Get-ChildItem tmp | Select-Object -First 1).Name) CDNTokenizers; rm tmp

ADD . Core

RUN nuget restore TvmCore/TvinciCore.sln
RUN nuget restore Core/Core.sln

RUN msbuild /p:Configuration=Release Core/Core.sln


