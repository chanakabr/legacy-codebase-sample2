FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as builder
WORKDIR /src

RUN apk add --no-cache git
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-trace
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-dump
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-counters
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-gcdump
RUN wget -qO ./grpc_health_probe https://github.com/grpc-ecosystem/grpc-health-probe/releases/download/v0.4.11/grpc_health_probe-linux-amd64
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
