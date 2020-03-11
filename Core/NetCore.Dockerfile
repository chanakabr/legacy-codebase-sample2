FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as builder
WORKDIR /src

RUN apt-get install git

ARG BRANCH=master
ARG API_VERSION

ENV TCM_URL http://tcm:8080/
ENV TCM_APP OTT_API_${API_VERSION}
ENV TCM_HOST main
ENV TCM_SECTION base
ENV TCM_APP_ID 5bf8cf60
ENV TCM_APP_SECRET 5aaa99331c18f6bad4adeef93ab770c2

COPY [".", "Core"]

WORKDIR /src/Core
RUN bash DllVersioning.Core.sh .
RUN bash get-version-tag.sh > version

RUN dotnet build -c  Release "./Core.sln"





