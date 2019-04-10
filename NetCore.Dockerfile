ARG BUILD_TAG=netcore-latest
FROM 870777418594.dkr.ecr.eu-west-1.amazonaws.com/core:${BUILD_TAG} AS builder

ARG BRANCH=master

WORKDIR /src
COPY [".", "RemoteTasks"]

WORKDIR /src/RemoteTasks

RUN bash /src/Core/DllVersioning.Core.sh .
RUN dotnet build -c Release "./RemoteTasksNetCore.sln" -o /src/published
