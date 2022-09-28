ARG BUILD_IMAGE=ott-be-base-build:latest
ARG RUN_IMAGE=ott-be-base-runtime:latest

FROM ${BUILD_IMAGE} as builder

WORKDIR /src/tvpapi
RUN sh /src/Core/DllVersioning.Core.sh . && \
    dotnet publish -c Release "./TVPApi.Api/TVPApi.Api.csproj" -o /src/published/tvpapi

FROM ${RUN_IMAGE}

ARG API_LOG_DIR=/var/log/tvpapi/

COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/published/tvpapi /opt/tvpapi
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/Core/GrpcClientCommon/lib/libgrpc_csharp_ext.x64.so /opt/tvpapi/runtimes/linux/native/libgrpc_csharp_ext.x64.so


WORKDIR /opt/tvpapi

USER kaltura
ENTRYPOINT ["sh", "-c", "dotnet TVPApi.Api.dll --urls http://0.0.0.0:${PORT}" ]