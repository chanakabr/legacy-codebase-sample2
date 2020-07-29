FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as builder
WORKDIR /src

RUN apk add --no-cache git

COPY [".git", ".git"]
COPY ["Core", "Core"]
COPY ["OPC_Migration", "OPC_Migration"]

#version patch
WORKDIR /src/Core
RUN sh DllVersioning.Core.sh . && \
    sh get-version-tag.sh > version

WORKDIR /src/OPC_Migration
RUN sh /src/Core/DllVersioning.Core.sh . && \
    dotnet publish -c Release "OPC_Migration.csproj" -o /src/published/opc_migration

# Cannot use alpine base runtime image because of this issue:
# https://github.com/dotnet/corefx/issues/29147
# Sql server will not connect on alpine, if this issue is resolved we should really switch to runtime:2.2-alpine
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

ARG USER_ID=1200
ARG GROUP_ID=1200
WORKDIR /opt


ARG API_LOG_DIR=/var/log/opc_migration/
ENV API_LOG_DIR ${API_LOG_DIR}
ENV API_STD_OUT_LOG_LEVEL "Off"

COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/published/opc_migration /opt/opc_migration
WORKDIR /opt/opc_migration

## Do NOT Remove This
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
##

###### deploy root CA ######
COPY consul-root-certificate.crt /usr/local/share/ca-certificates/consul-root-certificate.crt
RUN update-ca-certificates && \
    addgroup -g ${GROUP_ID} -S ott-users && \
    adduser -D -S -s /sbin/nologin -G ott-users kaltura -u ${USER_ID} && \
    mkdir -p ${API_LOG_DIR} && chown -R ${USER_ID}:${GROUP_ID} ${API_LOG_DIR} && \
    apk add --no-cache icu-libs && \
    apk add libgdiplus --no-cache --repository http://dl-3.alpinelinux.org/alpine/edge/testing/ --allow-untrusted

USER kaltura
##


ENTRYPOINT [ "sh", "-c", "dotnet OPC_Migration.dll" ]
