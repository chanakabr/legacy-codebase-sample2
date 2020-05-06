FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as builder
WORKDIR /src

RUN apt-get install git

COPY [".git", ".git"]
COPY ["Core", "Core"]
COPY ["phoenix", "phoenix"]

#version patch
WORKDIR /src/Core
RUN bash DllVersioning.Core.sh .
RUN bash get-version-tag.sh > version

WORKDIR /src/phoenix
RUN bash /src/Core/DllVersioning.Core.sh .
RUN dotnet publish -c Release "./Phoenix.Rest/Phoenix.Rest.csproj" -o /src/published/phoenix-rest

# Cannot use alpine base runtime image because of this issue:
# https://github.com/dotnet/corefx/issues/29147
# Sql server will not connect on alpine, if this issue is resolved we should really switch to runtime:2.2-alpine
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /opt

ARG API_LOG_DIR=/var/log/phoenix/
ENV API_LOG_DIR ${API_LOG_DIR}
ENV API_STD_OUT_LOG_LEVEL "Off"

COPY --from=builder /src/published/phoenix-rest /opt/phoenix-rest
WORKDIR /opt/phoenix-rest

###### deploy root CA ######
COPY consul-root-certificate.crt /usr/local/share/ca-certificates/consul-root-certificate.crt
RUN update-ca-certificates
###### deploy root CA ######

ENV ARGS "--urls http://0.0.0.0:80"

EXPOSE 80
EXPOSE 443

ENTRYPOINT [ "sh", "-c", "dotnet Phoenix.Rest.dll ${ARGS}" ]