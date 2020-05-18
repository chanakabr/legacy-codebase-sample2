FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as builder
WORKDIR /src

RUN apt-get install git

COPY [".git", ".git"]
COPY ["Core", "Core"]
COPY ["RemoteTasks", "RemoteTasks"]

#version patch
WORKDIR /src/Core
RUN bash DllVersioning.Core.sh .
RUN bash get-version-tag.sh > version

WORKDIR /src/RemoteTasks
RUN bash /src/Core/DllVersioning.Core.sh .

RUN dotnet publish -c Release "./HealthCheck/HealthCheck.csproj" -o /src/published/HealthCheck
RUN dotnet publish -c Release "./IngestHandler/IngestHandler.csproj" -o /src/published/IngestHandler
RUN dotnet publish -c Release "./IngestTransformationHandler/IngestTransformationHandler.csproj" -o /src/published/IngestTransformationHandler
RUN dotnet publish -c Release "./IngestValidationHandler/IngestValidationHandler.csproj" -o /src/published/IngestValidationHandler


# Cannot use alpine base runtime image because of this issue:
# https://github.com/dotnet/corefx/issues/29147
# Sql server will not connect on alpine, if this issue is resolved we should really switch to runtime:2.2-alpine
FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /

ENV RUN_TASK=no-task-selected
ENV CONCURRENT_CONSUMERS=1
ENV API_LOG_DIR=/var/log/remote-tasks/

COPY --from=builder /src/published .
###### deploy root CA ######
COPY consul-root-certificate.crt /usr/local/share/ca-certificates/consul-root-certificate.crt
RUN update-ca-certificates
###### deploy root CA ######
ENTRYPOINT [ "sh", "-c", "dotnet ./${RUN_TASK}/${RUN_TASK}.dll" ]

ARG VERSION
LABEL version=${VERSION}
