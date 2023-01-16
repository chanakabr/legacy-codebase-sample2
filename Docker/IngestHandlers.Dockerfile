ARG BUILD_IMAGE
ARG RUN_IMAGE

FROM ${BUILD_IMAGE} as builder

WORKDIR /src/RemoteTasks
RUN sh /src/Core/DllVersioning.Core.sh . && \
    dotnet publish -c Release "./HealthCheck/HealthCheck.csproj" -o /src/published/HealthCheck && \
    dotnet publish -c Release "./IngestHandler/IngestHandler.csproj" -o /src/published/IngestHandler && \
    dotnet publish -c Release "./IngestTransformationHandler/IngestTransformationHandler.csproj" -o /src/published/IngestTransformationHandler && \
    dotnet publish -c Release "./CampaignHandler/CampaignHandler.csproj" -o /src/published/CampaignHandler && \
    dotnet publish -c Release "./EpgNotificationHandler/EpgNotificationHandler.csproj" -o /src/published/EpgNotificationHandler && \
    dotnet publish -c Release "./LineupNotificationHandler/LineupNotificationHandler.csproj" -o /src/published/LineupNotificationHandler

FROM ${RUN_IMAGE}

ENV RUN_TASK=no-task-selected
ENV API_LOG_DIR=/var/log/ingesthandlers/

COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/published /opt/ingesthandlers
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/Core/GrpcClientCommon/lib/libgrpc_csharp_ext.x64.so /opt/ingesthandlers/HealthCheck/runtimes/linux/native/libgrpc_csharp_ext.x64.so
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/Core/GrpcClientCommon/lib/libgrpc_csharp_ext.x64.so /opt/ingesthandlers/IngestHandler/runtimes/linux/native/libgrpc_csharp_ext.x64.so
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/Core/GrpcClientCommon/lib/libgrpc_csharp_ext.x64.so /opt/ingesthandlers/IngestTransformationHandler/runtimes/linux/native/libgrpc_csharp_ext.x64.so
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/Core/GrpcClientCommon/lib/libgrpc_csharp_ext.x64.so /opt/ingesthandlers/CampaignHandler/runtimes/linux/native/libgrpc_csharp_ext.x64.so
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/Core/GrpcClientCommon/lib/libgrpc_csharp_ext.x64.so /opt/ingesthandlers/EpgNotificationHandler/runtimes/linux/native/libgrpc_csharp_ext.x64.so
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/Core/GrpcClientCommon/lib/libgrpc_csharp_ext.x64.so /opt/ingesthandlers/LineupNotificationHandler/runtimes/linux/native/libgrpc_csharp_ext.x64.so


WORKDIR /opt/ingesthandlers

USER kaltura
ENTRYPOINT [ "sh", "-c", "dotnet ./${RUN_TASK}/${RUN_TASK}.dll" ]
