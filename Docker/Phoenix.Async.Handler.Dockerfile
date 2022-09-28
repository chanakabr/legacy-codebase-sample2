ARG BUILD_IMAGE=ott-be-base-build:latest
ARG RUN_IMAGE=ott-be-base-runtime:latest
FROM ${BUILD_IMAGE} as builder

WORKDIR /src
RUN dotnet publish -c Release "./Phoenix.AsyncHandler/Phoenix.AsyncHandler.csproj" -o /src/published/ott-service-phoenix-async-handler

FROM ${RUN_IMAGE}

ENV API_LOG_DIR=/var/log/ott-service-phoenix-async-handler/

COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/published/ott-service-phoenix-async-handler /opt/ott-service-phoenix-async-handler
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/Core/GrpcClientCommon/lib/libgrpc_csharp_ext.x64.so /opt/ott-service-phoenix-async-handler/runtimes/linux/native/libgrpc_csharp_ext.x64.so


WORKDIR /opt/ott-service-phoenix-async-handler

USER kaltura
ENTRYPOINT ["./Phoenix.AsyncHandler"]