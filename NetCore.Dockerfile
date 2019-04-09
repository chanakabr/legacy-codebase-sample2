FROM microsoft/dotnet:2.2-sdk-alpine AS build
WORKDIR /src

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
RUN apk add unzip
RUN unzip TvmCore.zip -d ./uz_tvmcore
RUN mkdir TvmCore
RUN mv /src/uz_tvmcore/*/* /src/TvmCore/
RUN rm -rf uz_tvmcore

COPY [".", "Core"]

#RUN dotnet build -c Release "/src/Core/CoreNetCore.sln" --framework "netcoreapp2.0"





