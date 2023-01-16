ARG BUILD_IMAGE
ARG RUN_IMAGE

FROM ${BUILD_IMAGE} as builder

WORKDIR /src/SetupJobs
RUN sh /src/Core/DllVersioning.Core.sh . && dotnet publish -c Release "./Segmentation.Migrator/Segmentation.Migrator.csproj" -o /src/published/Segmentation.Migrator

FROM ${RUN_IMAGE}

ENV API_LOG_DIR=/var/log/segmentation_migrator/

COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/published /opt
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/Core/GrpcClientCommon/lib/libgrpc_csharp_ext.x64.so /opt/Segmentation.Migrator/runtimes/linux/native/libgrpc_csharp_ext.x64.so


WORKDIR /opt/Segmentation.Migrator

USER kaltura
ENTRYPOINT ["./Segmentation.Migrator"]
