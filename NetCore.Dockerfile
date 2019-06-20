FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine AS build
#FROM mcr.microsoft.com/dotnet/core/sdk:2.2 as builder
WORKDIR /src

# unzip required to unzip the downloaded TvmCore repo
RUN apk add unzip
# tools required for DllVersioning.Core.sh
RUN apk --update add grep
RUN apk --update add git
RUN apk --update add bash

ARG BRANCH=master
ARG BITBUCKET_TOKEN

ADD https://${BITBUCKET_TOKEN}@bitbucket.org/tvinci_dev/tvmcore/get/${BRANCH}.zip TvmCore.zip

RUN unzip TvmCore.zip -d ./uz_tvmcore
RUN mkdir TvmCore
RUN mv /src/uz_tvmcore/*/* /src/TvmCore/
RUN rm -rf uz_tvmcore
RUN rm -rf TvmCore.zip


COPY [".", "Core"]

WORKDIR /src/Core
RUN bash DllVersioning.Core.sh .
RUN bash DllVersioning.Core.sh ../TvmCore

RUN dotnet build -c Release "./CoreNetCore.sln"





