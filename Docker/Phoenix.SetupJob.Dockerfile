ARG BUILD_IMAGE
ARG RUN_IMAGE

FROM ${BUILD_IMAGE} as builder

WORKDIR /src
RUN sh /src/Core/DllVersioning.Core.sh . && \
    dotnet publish -c Release "./Phoenix.SetupJob/Phoenix.SetupJob.csproj" -o /src/published/ott-service-phoenix-setup-job

FROM ${RUN_IMAGE}

ARG API_LOG_DIR=/var/log/ott-service-phoenix-setup-job/

COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/published/ott-service-phoenix-setup-job /opt/ott-service-phoenix-setup-job
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/Core/GrpcClientCommon/lib/libgrpc_csharp_ext.x64.so /opt/ott-service-phoenix-setup-job/runtimes/linux/native/libgrpc_csharp_ext.x64.so

WORKDIR /opt/ott-service-phoenix-setup-job

USER kaltura
ENTRYPOINT ["./Phoenix.SetupJob"]
