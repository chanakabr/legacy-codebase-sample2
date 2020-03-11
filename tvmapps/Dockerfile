#ARG DOTNET_FRAMEWORK_TAG=4.7.2-sdk-windowsservercore-ltsc2019
#ARG IIS_TAG=windowsservercore-ltsc2019
#ARG BUILD_TAG=latest
#FROM microsoft/dotnet-framework:${DOTNET_FRAMEWORK_TAG} AS dotnetframework
#FROM 870777418594.dkr.ecr.eu-west-1.amazonaws.com/core:${BUILD_TAG} AS builder
#SHELL ["powershell"]
#
#ARG BRANCH=master
#ARG BITBUCKET_TOKEN
#
#ADD . tvmapps
#
#RUN nuget restore tvmapps/tvmapps.sln
#
#RUN msbuild /p:Configuration=Release tvmapps/tvmapps.sln
#RUN msbuild /t:WebPublish /p:Configuration=Release /p:DeployOnBuild=True /p:WebPublishMethod=FileSystem /p:PublishUrl=C:/tvmapps tvmapps/Web` Sites/TVM/website.publishproj

# core image
ARG IIS_TAG=windowsservercore-ltsc2019
ARG CORE_BUILD_TAG=latest
ARG CORE_IMAGE=870777418594.dkr.ecr.eu-west-1.amazonaws.com/core
FROM ${CORE_IMAGE}:${CORE_BUILD_TAG} AS builder
SHELL ["powershell"]
ADD . tvmapps
RUN nuget restore tvmapps/tvmapps.sln
RUN msbuild /p:Configuration=Release tvmapps/tvmapps.sln
RUN msbuild /t:WebPublish /p:Configuration=Release /p:DeployOnBuild=True /p:WebPublishMethod=FileSystem /p:PublishUrl=C:/tvmapps tvmapps/Web` Sites/TVM/website.publishproj


# tvmapps image
FROM mcr.microsoft.com/windows/servercore/iis:${IIS_TAG}
SHELL ["powershell"]

RUN Install-WindowsFeature NET-Framework-45-ASPNET
RUN Install-WindowsFeature Web-Asp-Net45

COPY --from=builder tvmapps tvmapps

RUN Remove-WebSite -Name 'Default Web Site'; \
	New-Website -Name tvmapps -Port 80 -PhysicalPath 'C:\tvmapps'

RUN $filePath = \"C:\WINDOWS\System32\Inetsrv\Config\applicationHost.config\"; \
	$doc = New-Object System.Xml.XmlDocument; \
	$doc.Load($filePath); \
	$child = $doc.CreateElement(\"logFile\"); \
	$child.SetAttribute(\"directory\", \"C:\log\iis\%COMPUTERNAME%\"); \
	$site = $doc.SelectSingleNode(\"//site[@name='tvmapps']\"); \
	$site.AppendChild($child); \
	$doc.Save($filePath)
	
RUN mkdir C:\log
RUN mkdir C:\log\api

ARG API_LOG_DIR=C:\\log\\api\\%COMPUTERNAME%
ENV API_LOG_DIR ${API_LOG_DIR}

RUN mv /tvmapps/ssl-dev /Certificate

RUN import-module webadministration; \
    $pwd = ConvertTo-SecureString -String "123456" -Force -AsPlainText; \
    $cert = Import-PfxCertificate -Password $pwd -FilePath \"C:\\Certificate\\cert.pfx\" -CertStoreLocation \"Cert:\LocalMachine\My\"; \
    New-Item -path IIS:\SslBindings\0.0.0.0!443 -value $cert; \
    New-WebBinding -Name "tvmapps" -IP "*" -Port 443 -Protocol https

EXPOSE 80
EXPOSE 443

RUN $version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo('C:\tvmapps\bin\tvmapps.dll').FileVersion; \
	Set-Content -Path 'C:\VERSION' -Value $version