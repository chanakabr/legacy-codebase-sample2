ARG BUILD_IMAGE
ARG RUN_IMAGE

FROM ${BUILD_IMAGE} as builder

WORKDIR /src/phoenix
RUN sh /src/Core/DllVersioning.Core.sh . && \
    dotnet publish -c Release "./Phoenix.Grpc/Phoenix.Grpc.csproj" -o /src/published/ott-service-phoenix-api-grpc

FROM ${RUN_IMAGE}

ENV API_LOG_DIR=/var/log/ott-service-phoenix-api-grpc/

COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/published/ott-service-phoenix-api-grpc /opt/ott-service-phoenix-api-grpc
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/grpc_health_probe /opt/ott-service-phoenix-api-grpc
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/Core/GrpcClientCommon/lib/libgrpc_csharp_ext.x64.so /opt/ott-service-phoenix-api-grpc/runtimes/linux/native/libgrpc_csharp_ext.x64.so


WORKDIR /opt/ott-service-phoenix-api-grpc

USER kaltura
ENTRYPOINT ["sh", "-c", "dotnet Phoenix.Grpc.dll --urls http://0.0.0.0:${PORT}" ]
