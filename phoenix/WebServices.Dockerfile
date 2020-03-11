ARG CORE_BUILD_TAG=netcore-latest
ARG CORE_IMAGE=870777418594.dkr.ecr.eu-west-1.amazonaws.com/core
FROM ${CORE_IMAGE}:${CORE_BUILD_TAG} AS builder

ARG BRANCH=master

WORKDIR /src
COPY [".", "phoenix-webservices"]
WORKDIR /src/phoenix-webservices

RUN bash /src/Core/DllVersioning.Core.sh .
RUN dotnet publish -c Release "./Phoenix.WebServices/Phoenix.WebServices.csproj" -o /src/published/phoenix-webservices

# Cannot use alpine base runtime image because of this issue:
# https://github.com/dotnet/corefx/issues/29147
# Sql server will not connect on alpine, if this issue is resolved we should really switch to runtime:2.2-alpine
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /opt

ARG API_LOG_DIR=/var/log/phoenix-webservices/
ENV API_LOG_DIR ${API_LOG_DIR}
ENV API_STD_OUT_LOG_LEVEL "Off"

COPY --from=builder /src/published/phoenix-webservices /opt/phoenix-webservices
WORKDIR /opt/phoenix-webservices

ENV ARGS "--urls http://0.0.0.0:80"

EXPOSE 80
EXPOSE 443

ENTRYPOINT [ "sh", "-c", "dotnet Phoenix.WebServices.dll ${ARGS}" ]


