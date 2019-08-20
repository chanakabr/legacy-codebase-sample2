ARG CORE_BUILD_TAG=netcore-latest
ARG CORE_IMAGE=870777418594.dkr.ecr.eu-west-1.amazonaws.com/core
FROM ${CORE_IMAGE}:${CORE_BUILD_TAG} AS builder

ARG BRANCH=master

WORKDIR /src
COPY [".", "phoenix-rest"]
WORKDIR /src/phoenix-rest

RUN bash /src/Core/DllVersioning.Core.sh .
RUN dotnet publish -c Release "./Phoenix.Rest/Phoenix.Rest.csproj" -o /src/published/phoenix-rest

# Cannot use alpine base runtime image because of this issue:
# https://github.com/dotnet/corefx/issues/29147
# Sql server will not connect on alpine, if this issue is resolved we should really switch to runtime:2.2-alpine
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /opt

ARG API_LOG_DIR=/var/log/phoenix/
ENV API_LOG_DIR ${API_LOG_DIR}
ENV API_STD_OUT_LOG_LEVEL "Trace"

COPY --from=builder /src/published/phoenix-rest /opt/phoenix-rest
WORKDIR /opt/phoenix-rest

ENV ARGS "--urls http://0.0.0.0:80"

EXPOSE 80
EXPOSE 443

ENTRYPOINT [ "sh", "-c", "dotnet Phoenix.Rest.dll ${ARGS}" ]


