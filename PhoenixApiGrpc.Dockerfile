# .net container - with sdk. This is our builder.
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as builder
WORKDIR /src

RUN apk add --no-cache git

COPY [".git", ".git"]
COPY ["Core", "Core"]
COPY ["phoenix", "phoenix"]

RUN wget -qO ./grpc_health_probe https://github.com/grpc-ecosystem/grpc-health-probe/releases/download/v0.3.2/grpc_health_probe-linux-amd64
RUN chmod +x ./grpc_health_probe

#version patch
WORKDIR /src/Core
RUN sh DllVersioning.Core.sh . && \
    sh get-version-tag.sh > version

WORKDIR /src/phoenix

# publish and output to /src/publish
RUN dotnet publish -c Release "./Phoenix.Grpc/Phoenix.Grpc.csproj" -o /src/published

# .net container with no sdk - runtime container
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

# install glibc for grpc to work on alpine
ENV GLIBC_REPO=https://github.com/sgerrand/alpine-pkg-glibc
ENV GLIBC_VERSION=2.32-r0

RUN set -ex && \
    apk --update add libstdc++ curl ca-certificates && \
    for pkg in glibc-${GLIBC_VERSION} glibc-bin-${GLIBC_VERSION}; \
        do curl -sSL ${GLIBC_REPO}/releases/download/${GLIBC_VERSION}/${pkg}.apk -o /tmp/${pkg}.apk; done && \
    apk add --allow-untrusted /tmp/*.apk && \
    rm -v /tmp/*.apk && \
    /usr/glibc-compat/sbin/ldconfig /lib /usr/glibc-compat/lib

# install this for rdkafka to wotk on alpine with glibc
RUN apk add librdkafka librdkafka-dev

# user's group = non-root users, so the docker won't have elevated permissions (security consideration)
ARG USER_ID=1200
ARG GROUP_ID=1200

ENV PORT 8080

WORKDIR /opt/ott-service-phoenix-api-grpc

# KLogMonitor standart environment variable
ENV API_LOG_DIR=/var/log/ott-service-phoenix-api-grpc/

# chown = change owner
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/published .
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/grpc_health_probe .
# copy precompiled grpc csharp wrapper custom for this machine
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/Core/GrpcClientCommon/lib/libgrpc_csharp_ext.x64.so /opt/ott-service-phoenix-api-grpc/runtimes/linux/native/libgrpc_csharp_ext.x64.so

## Do NOT Remove This, this is needed to support CultureInfo.CurrentCulture in .net, and unfortenatlly we are using it
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
##

###### deploy root CA + add user######
COPY consul-root-certificate.crt /usr/local/share/ca-certificates/consul-root-certificate.crt
RUN update-ca-certificates && \
    addgroup -g ${GROUP_ID} -S ott-users && \
    adduser -D -S -s /sbin/nologin -G ott-users kaltura -u ${USER_ID} && \
    mkdir -p ${API_LOG_DIR} && chown -R ${USER_ID}:${GROUP_ID} ${API_LOG_DIR} && \
    apk add --no-cache icu-libs curl less  && \
    apk add libgdiplus --no-cache --repository http://dl-3.alpinelinux.org/alpine/edge/testing/ --allow-untrusted

USER kaltura
###### deploy root CA ######

ENTRYPOINT [ "sh", "-c", "dotnet ./Phoenix.Grpc.dll --urls http://0.0.0.0:${PORT}" ]