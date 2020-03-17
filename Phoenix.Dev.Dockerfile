FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as builder
WORKDIR /src

RUN apt-get install git
RUN dotnet tool install --global dotnet-counters
ENV PATH="${PATH}:/root/.dotnet/tools"

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

ARG API_LOG_DIR=/var/log/phoenix/
ENV API_LOG_DIR ${API_LOG_DIR}
ENV API_STD_OUT_LOG_LEVEL "Off"

WORKDIR /opt/phoenix-rest
RUN cp -r /src/published/phoenix-rest /opt

ENV ARGS "--urls http://0.0.0.0:80"

EXPOSE 80
EXPOSE 443

ENTRYPOINT [ "sh", "-c", "dotnet Phoenix.Rest.dll ${ARGS}" ]






