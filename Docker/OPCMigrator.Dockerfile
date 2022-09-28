ARG BUILD_IMAGE=ott-be-base-build:latest
ARG RUN_IMAGE=ott-be-base-runtime:latest

FROM ${BUILD_IMAGE} as builder


WORKDIR /src/OPC_Migration
RUN sh /src/Core/DllVersioning.Core.sh . && \
    dotnet publish -c Release "OPC_Migration.csproj" -o /src/published/opc_migration

FROM ${RUN_IMAGE}

ARG API_LOG_DIR=/var/log/opc_migration/

COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/published/opc_migration /opt/opc_migration

WORKDIR /opt/opc_migration

USER kaltura
ENTRYPOINT ["./dotnet OPC_Migration"]