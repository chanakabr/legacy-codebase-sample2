ARG BUILD_IMAGE=ott-be-base-build:latest
ARG RUN_IMAGE=ott-be-base-runtime:latest

FROM ${BUILD_IMAGE} as builder

WORKDIR /src/SetupJobs
RUN sh /src/Core/DllVersioning.Core.sh . && dotnet publish -c Release "./Epg.V3.Rollback/Epg.V3.Rollback.csproj" -o /src/published/Epg.V3.Rollback 

FROM ${RUN_IMAGE}

ENV API_LOG_DIR=/var/log/epg_v3_rollback/

COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/published .
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/Core/GrpcClientCommon/lib/libgrpc_csharp_ext.x64.so /opt/Epg.V3.Rollback/runtimes/linux/native/libgrpc_csharp_ext.x64.so


WORKDIR /opt/Epg.V3.Rollback

USER kaltura
ENTRYPOINT ["./Epg.V3.Rollback"]