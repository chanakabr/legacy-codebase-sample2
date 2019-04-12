ARG CORE_BUILD_TAG=netcore-latest
FROM 870777418594.dkr.ecr.eu-west-1.amazonaws.com/core:${CORE_BUILD_TAG} AS builder

ARG BRANCH=master

WORKDIR /src
COPY [".", "RemoteTasks"]

WORKDIR /src/RemoteTasks

RUN bash /src/Core/DllVersioning.Core.sh .
RUN dotnet publish -c Release "./RemoteTasksNetCore.sln" -o /src/published

FROM mcr.microsoft.com/dotnet/core/runtime:2.2-alpine
WORKDIR /

ENV RUN_TASK=no-task-selected
ENV CONCURRENT_CONSUMERS=4
ENV API_LOG_DIR=logs/

COPY --from=builder /src/published .
ENTRYPOINT [ "sh", "-c", "dotnet ./${RUN_TASK}.dll" ]
