ARG BUILD_IMAGE
ARG RUN_IMAGE

FROM ${BUILD_IMAGE} as builder

WORKDIR /src/WS_Ingest
RUN sh /src/Core/DllVersioning.Core.sh . && \
    dotnet publish -c Release "./IngestNetCore/IngestNetCore.csproj" -o /src/published/ws-ingest

FROM ${RUN_IMAGE}

ARG API_LOG_DIR=/var/log/ws-ingest/

COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/published/ws-ingest /opt/ws-ingest
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/Core/GrpcClientCommon/lib/libgrpc_csharp_ext.x64.so /opt/ws-ingest/runtimes/linux/native/libgrpc_csharp_ext.x64.so


WORKDIR /opt/ws-ingest

USER kaltura
ENTRYPOINT ["sh", "-c", "dotnet IngestNetCore.dll --urls http://0.0.0.0:${PORT}" ]
