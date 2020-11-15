FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as builder
WORKDIR /src

RUN apk add --no-cache git

COPY [".git", ".git"]
COPY ["Core", "Core"]
COPY ["WS_Ingest", "WS_Ingest"]

#version patch
WORKDIR /src/Core
RUN sh DllVersioning.Core.sh . && \
    sh get-version-tag.sh > version

WORKDIR /src/WS_Ingest
RUN sh /src/Core/DllVersioning.Core.sh . && \
    dotnet publish -c Release "./IngestNetCore/IngestNetCore.csproj" -o /src/published/ws-ingest

# Cannot use alpine base runtime image because of this issue:
# https://github.com/dotnet/corefx/issues/29147
# Sql server will not connect on alpine, if this issue is resolved we should really switch to runtime:2.2-alpine
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
ARG USER_ID=1200
ARG GROUP_ID=1200
WORKDIR /opt

#RUN apt update && apt install -y libc6-dev libgdiplus

ARG API_LOG_DIR=/var/log/ws-ingest/
ENV API_LOG_DIR ${API_LOG_DIR}
ENV API_STD_OUT_LOG_LEVEL "Off"
## Do NOT Remove This
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
##
ENV PORT 8080

COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/published/ws-ingest /opt/ws-ingest
WORKDIR /opt/ws-ingest

###### deploy root CA + add user######
COPY consul-root-certificate.crt /usr/local/share/ca-certificates/consul-root-certificate.crt
RUN update-ca-certificates && \
    addgroup -g ${GROUP_ID} -S ott-users && \
    adduser -D -S -s /sbin/nologin -G ott-users kaltura -u ${USER_ID} && \
    mkdir -p ${API_LOG_DIR} && chown -R ${USER_ID}:${GROUP_ID} ${API_LOG_DIR} && \
    apk add --no-cache icu-libs curl less libc6-compat && \
    apk add --no-cache icu-libs curl less && \
    apk add libgdiplus --no-cache --repository http://dl-3.alpinelinux.org/alpine/edge/testing/ --allow-untrusted

USER kaltura
###### deploy root CA ######


ENTRYPOINT [ "sh", "-c", "dotnet IngestNetCore.dll --urls http://0.0.0.0:${PORT}" ]
