ARG BUILD_IMAGE
ARG RUN_IMAGE
FROM ${BUILD_IMAGE} as builder

WORKDIR /src/phoenix
RUN sh /src/Core/DllVersioning.Core.sh . && \
    dotnet publish -c Release "./Phoenix.Rest/Phoenix.Rest.csproj" -o /src/published/phoenix-rest

FROM ${RUN_IMAGE}

ARG API_LOG_DIR=/var/log/phoenix/

COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/published/phoenix-rest /opt/phoenix-rest
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/Core/GrpcClientCommon/lib/libgrpc_csharp_ext.x64.so /opt/phoenix-rest/runtimes/linux/native/libgrpc_csharp_ext.x64.so

WORKDIR /opt/phoenix-rest

USER kaltura
ENTRYPOINT ["sh", "-c", "dotnet Phoenix.Rest.dll --urls http://0.0.0.0:${PORT}" ]
