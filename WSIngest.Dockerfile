FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as builder
WORKDIR /src

RUN apt-get install git

COPY [".git", ".git"]
COPY ["Core", "Core"]
COPY ["WS_Ingest", "WS_Ingest"]

#version patch
WORKDIR /src/Core
RUN bash DllVersioning.Core.sh .
RUN bash get-version-tag.sh > version

WORKDIR /src/WS_Ingest
RUN bash /src/Core/DllVersioning.Core.sh .
RUN dotnet publish -c Release "./IngestNetCore/IngestNetCore.csproj" -o /src/published/ws-ingest

# Cannot use alpine base runtime image because of this issue:
# https://github.com/dotnet/corefx/issues/29147
# Sql server will not connect on alpine, if this issue is resolved we should really switch to runtime:2.2-alpine
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
ARG USER_ID=1200
ARG GROUP_ID=1200
WORKDIR /opt

RUN apt update && apt install -y libc6-dev libgdiplus

ARG API_LOG_DIR=/var/log/ws-ingest/
ENV API_LOG_DIR ${API_LOG_DIR}
ENV API_STD_OUT_LOG_LEVEL "Off"

COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/published/ws-ingest /opt/ws-ingest
WORKDIR /opt/ws-ingest

###### deploy root CA ######
COPY consul-root-certificate.crt /usr/local/share/ca-certificates/consul-root-certificate.crt
RUN update-ca-certificates
###### deploy root CA ######

RUN groupadd -g ${GROUP_ID} ott-users && \
    useradd -g ${GROUP_ID} -u ${USER_ID} kaltura -s /sbin/nologin && \
    mkdir -p ${API_LOG_DIR} && chown -R ${USER_ID}:${GROUP_ID} ${API_LOG_DIR}
USER kaltura

ENV PORT 8080

EXPOSE 80
EXPOSE 443

ENTRYPOINT [ "sh", "-c", "dotnet IngestNetCore.dll --urls http://0.0.0.0:${PORT}" ]
