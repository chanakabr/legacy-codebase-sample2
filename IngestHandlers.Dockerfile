FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as builder
WORKDIR /src

RUN apk add --no-cache git

COPY [".git", ".git"]
COPY ["Core", "Core"]
COPY ["RemoteTasks", "RemoteTasks"]

#version patch
WORKDIR /src/Core
RUN sh DllVersioning.Core.sh . && \
    sh get-version-tag.sh > version

WORKDIR /src/RemoteTasks
RUN sh /src/Core/DllVersioning.Core.sh . && \
    dotnet publish -c Release "./HealthCheck/HealthCheck.csproj" -o /src/published/HealthCheck && \
    dotnet publish -c Release "./IngestHandler/IngestHandler.csproj" -o /src/published/IngestHandler && \
    dotnet publish -c Release "./IngestTransformationHandler/IngestTransformationHandler.csproj" -o /src/published/IngestTransformationHandler && \
	dotnet publish -c Release "./CampaignHandler/CampaignHandler.csproj" -o /src/published/CampaignHandler


# Cannot use alpine base runtime image because of this issue:
# https://github.com/dotnet/corefx/issues/29147
# Sql server will not connect on alpine, if this issue is resolved we should really switch to runtime:2.2-alpine
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
ARG USER_ID=1200
ARG GROUP_ID=1200
WORKDIR /opt

ENV RUN_TASK=no-task-selected
ENV CONCURRENT_CONSUMERS=1
ENV API_LOG_DIR=/var/log/ingesthandlers/

COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/published .

## Do NOT Remove This
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
##

###### deploy root CA + add user######
COPY consul-root-certificate.crt /usr/local/share/ca-certificates/consul-root-certificate.crt
RUN update-ca-certificates && \
    addgroup -g ${GROUP_ID} -S ott-users && \
    adduser -D -S -s /sbin/nologin -G ott-users kaltura -u ${USER_ID} && \
    mkdir -p ${API_LOG_DIR} && chown -R ${USER_ID}:${GROUP_ID} ${API_LOG_DIR} && \
    apk add --no-cache icu-libs curl less libc6-compat

USER kaltura
###### deploy root CA ######

ENTRYPOINT [ "sh", "-c", "dotnet ./${RUN_TASK}/${RUN_TASK}.dll" ]

ARG VERSION
LABEL version=${VERSION}
