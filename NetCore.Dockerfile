FROM mcr.microsoft.com/dotnet/core/sdk:2.2 as builder
WORKDIR /src

RUN apt-get install git

ARG BRANCH=master
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

COPY [".", "Core"]

WORKDIR /src/Core
RUN bash DllVersioning.Core.sh .
RUN bash get-version-tag.sh > version

RUN dotnet build -c  Release "./Core.sln"





