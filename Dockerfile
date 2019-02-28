ARG BUILD_TAG=latest
ARG IIS_TAG=windowsservercore-ltsc2019
FROM 870777418594.dkr.ecr.eu-west-1.amazonaws.com/core:${BUILD_TAG} AS builder
SHELL ["powershell"]

ARG BRANCH=master
ARG BITBUCKET_TOKEN
ARG GITHUB_TOKEN

ADD . ws_ingest

RUN nuget restore ws_ingest/ws_ingest.sln
RUN msbuild /p:Configuration=Release ws_ingest/ws_ingest.sln

RUN msbuild /t:WebPublish /p:Configuration=Release /p:DeployOnBuild=True /p:WebPublishMethod=FileSystem /p:PublishUrl=C:/Ingest ws_ingest/Ingest/Ingest.csproj










FROM mcr.microsoft.com/windows/servercore/iis:${IIS_TAG}
SHELL ["powershell"]

RUN Install-WindowsFeature NET-Framework-45-ASPNET
RUN Install-WindowsFeature Web-Asp-Net45
RUN Add-WindowsFeature NET-WCF-HTTP-Activation45

COPY --from=builder Ingest Ingest

RUN Remove-WebSite -Name 'Default Web Site'; \
	New-Website -Name Ingest -Port 80 -PhysicalPath 'C:\Ingest'
	
RUN $filePath = \"C:\WINDOWS\System32\Inetsrv\Config\applicationHost.config\"; \
	$doc = New-Object System.Xml.XmlDocument; \
	$doc.Load($filePath); \
	$child = $doc.CreateElement(\"logFile\"); \
	$child.SetAttribute(\"directory\", \"C:\log\iis\%COMPUTERNAME%\"); \
	$site = $doc.SelectSingleNode(\"//site[@name='Ingest']\"); \
	$site.AppendChild($child); \
	$doc.Save($filePath)
	
ARG INGEST_LOG_DIR=C:\\log\\ingest\\%COMPUTERNAME%
ENV INGEST_LOG_DIR ${INGEST_LOG_DIR}

RUN mv Ingest\\ssl-dev C:\\Certificate

RUN import-module webadministration; \
    $pwd = ConvertTo-SecureString -String "123456" -Force -AsPlainText; \
    $cert = Import-PfxCertificate -Password $pwd -FilePath \"C:\\Certificate\\cert.pfx\" -CertStoreLocation \"Cert:\LocalMachine\My\"; \
    New-Item -path IIS:\SslBindings\0.0.0.0!443 -value $cert; \
    New-WebBinding -Name "Ingest" -IP "*" -Port 443 -Protocol https

EXPOSE 80
EXPOSE 443

RUN $version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo('C:\Ingest\bin\Ingest.dll').FileVersion; \
	Set-Content -Path 'C:\VERSION' -Value $version
	