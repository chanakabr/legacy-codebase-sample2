ARG BUILD_TAG=netcore-latest
FROM 870777418594.dkr.ecr.eu-west-1.amazonaws.com/core:${BUILD_TAG} AS builder

ARG BRANCH=master

WORKDIR /src
COPY [".", "RemoteTasks"]

WORKDIR /src/RemoteTasks

RUN bash /src/Core/DllVersioning.Core.sh .
RUN dotnet build -c Release "./RemoteTasksNetCore.sln" -o /src/published

COPY --from=builder /usr/local/opc/tvm-ng/dist /usr/local/opc

FROM mcr.microsoft.com/dotnet/core/runtime:2.2-alpine
ENV RUN_TASK=no-task-selected

WORKDIR /
COPY --from=builder /src/published .
ENTRYPOINT [ "sh", "-c", "dotnet $RUN_TASK" ]
