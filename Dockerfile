ARG CORE_BUILD_TAG=netcore-latest
ARG CORE_IMAGE=870777418594.dkr.ecr.eu-west-1.amazonaws.com/core
FROM ${CORE_IMAGE}:${CORE_BUILD_TAG} AS builder

ARG BRANCH=master

WORKDIR /
COPY /Phoenix.Rest/Phoenix.Rest.csproj .
RUN dotnet restore Phoenix.Rest.csproj
COPY packages packages
RUN cp `find packages -name crossgen` .
RUN dotnet restore


COPY [".", "/src/phoenix-rest/"]
WORKDIR /src/phoenix-rest

RUN bash /src/Core/DllVersioning.Core.sh .
RUN dotnet publish -c Release "./Phoenix.Rest/Phoenix.Rest.csproj" -o /src/published/phoenix-rest

# Cannot use alpine base runtime image because of this issue:
# https://github.com/dotnet/corefx/issues/29147
# Sql server will not connect on alpine, if this issue is resolved we should really switch to runtime:2.2-alpine
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0
WORKDIR /opt

ARG API_LOG_DIR=/var/log/phoenix/
ENV API_LOG_DIR ${API_LOG_DIR}
ENV API_STD_OUT_LOG_LEVEL "Off"

COPY --from=builder /src/published/phoenix-rest /opt/phoenix-rest
WORKDIR /opt/phoenix-rest

ENV ARGS "--urls http://0.0.0.0:80"

EXPOSE 80
EXPOSE 443

ENTRYPOINT [ "sh", "-c", "dotnet Phoenix.Rest.dll ${ARGS}" ]


FROM app as sidecar
# Add whatever tools you want here
RUN apt-get update \
    && apt-get install -y \
       binutils \
       curl \
       htop \
       procps \
       liblttng-ust-dev \
       linux-tools \
       lttng-tools \
       zip \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /tools

RUN curl -OL http://aka.ms/perfcollect \
    && chmod a+x perfcollect

# perfcollect expects to find crossgen along side libcoreclr.so
RUN cp /app/crossgen $(dirname `find /usr/share/dotnet/ -name libcoreclr.so`)

ENTRYPOINT ["/bin/bash"]

