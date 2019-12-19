ARG CORE_BUILD_TAG=netcore-latest
ARG CORE_IMAGE=870777418594.dkr.ecr.eu-west-1.amazonaws.com/core
FROM ${CORE_IMAGE}:${CORE_BUILD_TAG} AS builder

ARG BRANCH=master

WORKDIR /src
COPY [".", "ws-ingest"]
WORKDIR /src/ws-ingest

RUN bash /src/Core/DllVersioning.Core.sh .
RUN dotnet publish -c Release "./IngestNetCore/IngestNetCore.csproj" -o /src/published/ws-ingest

# Cannot use alpine base runtime image because of this issue:
# https://github.com/dotnet/corefx/issues/29147
# Sql server will not connect on alpine, if this issue is resolved we should really switch to runtime:2.2-alpine
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /opt

ARG API_LOG_DIR=/var/log/ws-ingest/
ENV API_LOG_DIR ${API_LOG_DIR}
ENV API_STD_OUT_LOG_LEVEL "Off"

COPY --from=builder /src/published/ws-ingest /opt/ws-ingest
WORKDIR /opt/ws-ingest

ENV ARGS "--urls http://0.0.0.0:80"

EXPOSE 80
EXPOSE 443

ENTRYPOINT [ "sh", "-c", "dotnet IngestNetCore.dll ${ARGS}" ]


