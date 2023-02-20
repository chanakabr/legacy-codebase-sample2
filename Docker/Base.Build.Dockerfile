FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as builder
WORKDIR /src

RUN apk add --no-cache git
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-trace --version 6.0.351802
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-dump --version 6.0.351802
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-counters --version 6.0.351802
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-gcdump --version 6.0.351802
RUN wget -qO ./grpc_health_probe https://github.com/grpc-ecosystem/grpc-health-probe/releases/download/v0.4.14/grpc_health_probe-linux-amd64
RUN chmod +x ./grpc_health_probe


COPY [".git", ".git"]
COPY [".", "."]

#version patch
WORKDIR /src/Core
RUN sh DllVersioning.Core.sh . && sh get-version-tag.sh > version

WORKDIR /src
RUN sh /src/Core/DllVersioning.Core.sh ..
RUN dotnet build -c Release "./Core/Logic/ApiLogic.csproj"
RUN chmod -R 777 /src
RUN chmod -R 777 /tmp
