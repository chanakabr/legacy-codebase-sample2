ARG CORE_BUILD_TAG=netcore-latest
ARG CORE_IMAGE=870777418594.dkr.ecr.eu-west-1.amazonaws.com/core
FROM ${CORE_IMAGE}:${CORE_BUILD_TAG} AS builder

ARG BRANCH=master

WORKDIR /src
COPY [".", "RemoteTasks"]

WORKDIR /src/RemoteTasks

RUN bash /src/Core/DllVersioning.Core.sh .
RUN dotnet publish -c Release "./RemoteTasksNetCore.sln" -o /src/published

# Cannot use alpine base runtime image because of this issue:
# https://github.com/dotnet/corefx/issues/29147
# Sql server will not connect on alpine, if this issue is resolved we should really switch to runtime:2.2-alpine
FROM mcr.microsoft.com/dotnet/core/runtime:2.2
WORKDIR /

ENV RUN_TASK=no-task-selected
ENV CONCURRENT_CONSUMERS=4
ENV API_LOG_DIR=/var/log/remote-tasks/

COPY --from=builder /src/published .
ENTRYPOINT [ "sh", "-c", "dotnet ./${RUN_TASK}.dll" ]
