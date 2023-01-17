ARG BUILD_IMAGE
FROM ${BUILD_IMAGE} as builder

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

# install common tools
RUN apk add tiff --no-cache --repository https://dl-3.alpinelinux.org/alpine/edge/main --force-overwrite --allow-untrusted
RUN apk add libgdiplus --no-cache --repository https://dl-3.alpinelinux.org/alpine/edge/community/ --force-overwrite --allow-untrusted
RUN apk add curl less --no-cache --repository http://dl-3.alpinelinux.org/alpine/edge/testing/ --force-overwrite --allow-untrusted

# install glibc for grpc to work on alpine
ENV GLIBC_REPO=https://github.com/sgerrand/alpine-pkg-glibc
ENV GLIBC_VERSION=2.32-r0

RUN set -ex && \
    apk --update add libstdc++ curl ca-certificates && \
    for pkg in glibc-${GLIBC_VERSION} glibc-bin-${GLIBC_VERSION}; \
        do curl -sSL ${GLIBC_REPO}/releases/download/${GLIBC_VERSION}/${pkg}.apk -o /tmp/${pkg}.apk; done && \
    apk add --force-overwrite --allow-untrusted /tmp/*.apk && \
    rm -v /tmp/*.apk && \
    /usr/glibc-compat/sbin/ldconfig /lib /usr/glibc-compat/lib

## Do NOT Remove This - Required for dotnet OTT-BE sql connection
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs
##

###### deploy root CA + add user######
ARG USER_ID=1200
ARG GROUP_ID=1200

ENV USER_ID=${USER_ID}
ENV GROUP_ID=${GROUP_ID}

COPY consul-root-certificate.crt /usr/local/share/ca-certificates/consul-root-certificate.crt
RUN update-ca-certificates && addgroup -g ${GROUP_ID} -S ott-users && \
    adduser -D -S -s /sbin/nologin -G ott-users kaltura -u ${USER_ID}


# copy dotnet tools tools
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /dotnetcore-tools /dotnetcore-tools

WORKDIR /opt/app

# default port and urls for all kesterl based apps
ENV PORT 8080
ENV DOTNET_URLS http://0.0.0.0:${PORT}
ENV ASPNETCORE_URLS http://0.0.0.0:${PORT}

#default stdout logs
ENV API_STD_OUT_LOG_LEVEL "INFO"

