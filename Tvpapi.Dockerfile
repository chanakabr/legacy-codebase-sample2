FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as builder
WORKDIR /src

RUN apt-get install git

COPY [".git", ".git"]
COPY ["Core", "Core"]
COPY ["tvpapi", "tvpapi"]

#version patch
WORKDIR /src/Core
RUN bash DllVersioning.Core.sh .
RUN bash get-version-tag.sh > version

WORKDIR /src/tvpapi
RUN bash /src/Core/DllVersioning.Core.sh .
RUN dotnet publish -c Release "./TVPApi.Api/TVPApi.Api.csproj" -o /src/published/tvpapi

# Cannot use alpine base runtime image because of this issue:
# https://github.com/dotnet/corefx/issues/29147
# Sql server will not connect on alpine, if this issue is resolved we should really switch to runtime:2.2-alpine
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /opt

ARG API_LOG_DIR=/var/log/tvpapi/
ENV API_LOG_DIR ${API_LOG_DIR}
ENV API_STD_OUT_LOG_LEVEL "Off"

COPY --from=builder /src/published/tvpapi /opt/tvpapi
WORKDIR /opt/tvpapi

###### locale change ######
RUN apt-get update && DEBIAN_FRONTEND=noninteractive apt-get install -y locales

RUN sed -i -e 's/# en_US.UTF-8 UTF-8/en_US.UTF-8 UTF-8/' /etc/locale.gen && \
    dpkg-reconfigure --frontend=noninteractive locales && \
    update-locale LANG=en_US.UTF-8

ENV LANG en_US.UTF-8
###### locale change ######

###### deploy root CA ######
COPY consul-root-certificate.crt /usr/local/share/ca-certificates/consul-root-certificate.crt
RUN update-ca-certificates
###### deploy root CA ######

ENV ARGS "--urls http://0.0.0.0:80"

EXPOSE 80
EXPOSE 443

ENTRYPOINT [ "sh", "-c", "dotnet TVPApi.Api.dll ${ARGS}" ]
