ARG DOTNET_FRAMEWORK_TAG=4.7.2-sdk-windowsservercore-ltsc2019
FROM microsoft/dotnet-framework:${DOTNET_FRAMEWORK_TAG} AS builder
SHELL ["powershell"]

ARG BRANCH=master
ARG API_VERSION

ARG TCM_URL=http://tcm:8080/
ARG TCM_APP=OTT_API_${API_VERSION}
ARG TCM_HOST=main
ARG TCM_SECTION=base
ARG TCM_APP_ID=5bf8cf60
ARG TCM_APP_SECRET=5aaa99331c18f6bad4adeef93ab770c2

ENV TCM_URL ${TCM_URL}
ENV TCM_APP ${TCM_APP}
ENV TCM_HOST ${TCM_HOST}
ENV TCM_SECTION ${TCM_SECTION}
ENV TCM_APP_ID ${TCM_APP_ID}
ENV TCM_APP_SECRET ${TCM_APP_SECRET}

WORKDIR /

# Install choco and Git for versioning script
RUN Set-ExecutionPolicy Bypass -Scope Process -Force; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
RUN choco install -y git

ADD . Core
WORKDIR /Core
RUN  ./DllVersioning.ps1 .

RUN msbuild -p:Configuration=Release -t:restore,build  Core.sln

WORKDIR /

