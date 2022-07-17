# .net container - with sdk. This is our builder.
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as builder
WORKDIR /src

# copy all project to working directory
COPY [".", "."]

# publish and output to /src/publish
RUN dotnet publish -c Release "./Phoenix.SetupJob/Phoenix.SetupJob.csproj" -o /src/published

# .net container with no sdk - runtime container
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
# user's group = non-root users, so the docker won't have elevated permissions (security consideration)
ARG USER_ID=1200
ARG GROUP_ID=1200

WORKDIR /opt/ott-service-phoenix-setup-job

# KLogMonitor standart environment variable
ENV API_LOG_DIR=/var/log/ott-service-phoenix-setup-job/

# chown = change owner
COPY --chown=${USER_ID}:${GROUP_ID} --from=builder /src/published .

## Do NOT Remove This, this is needed to support CultureInfo.CurrentCulture in .net, and unfortenatlly we are using it
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
##

USER kaltura
ENTRYPOINT ["dotnet", "Phoenix.SetupJob.dll"]

# version will be with a build argument
ARG VERSION
LABEL version=${VERSION}
