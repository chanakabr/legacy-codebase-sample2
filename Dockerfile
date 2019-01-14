ARG DOTNET_FRAMEWORK_TAG=4.7.2-sdk-windowsservercore-ltsc2019
ARG IIS_TAG=windowsservercore-ltsc2019
FROM microsoft/dotnet-framework:${DOTNET_FRAMEWORK_TAG} AS builder
SHELL ["powershell"]

ARG BRANCH=master
ARG BITBUCKET_TOKEN
ARG GITHUB_TOKEN
ARG API_VERSION

ARG TCM_URL=http://tcm:8080/
ARG TCM_APP=OTT_API_${API_VERSION}
ARG TCM_SECTION=base
ARG TCM_APP_ID=5bf8cf60
ARG TCM_APP_SECRET=5aaa99331c18f6bad4adeef93ab770c2

ENV TCM_URL ${TCM_URL}
ENV TCM_APP ${TCM_APP}
ENV TCM_SECTION ${TCM_SECTION}
ENV TCM_APP_ID ${TCM_APP_ID}
ENV TCM_APP_SECRET ${TCM_APP_SECRET}

ADD https://${BITBUCKET_TOKEN}@bitbucket.org/tvinci_dev/tvmcore/get/${BRANCH}.zip TvmCore.zip
ADD https://${BITBUCKET_TOKEN}@bitbucket.org/tvinci_dev/CDNTokenizers/get/${BRANCH}.zip CDNTokenizers.zip
ADD https://api.github.com/repos/kaltura/Core/zipball/${BRANCH}?access_token=${GITHUB_TOKEN} Core.zip

RUN Expand-Archive TvmCore.zip -DestinationPath tmp; mv tmp/$((Get-ChildItem tmp | Select-Object -First 1).Name) TvmCore; rm tmp; rm TvmCore.zip
RUN Expand-Archive CDNTokenizers.zip -DestinationPath tmp; mv tmp/$((Get-ChildItem tmp | Select-Object -First 1).Name) CDNTokenizers; rm tmp; rm CDNTokenizers.zip
RUN Expand-Archive Core.zip -DestinationPath tmp; mv tmp/$((Get-ChildItem tmp | Select-Object -First 1).Name) Core; rm tmp; rm Core.zip

ADD . tvpapi_rest

RUN nuget restore tvpapi_rest/TVPProAPIs.sln
RUN nuget restore TvmCore/TvinciCore.sln
RUN nuget restore Core/Core.sln

RUN msbuild /p:Configuration=Release tvpapi_rest/TVPProAPIs.sln
RUN msbuild /t:WebPublish /p:Configuration=Release /p:DeployOnBuild=True /p:WebPublishMethod=FileSystem /p:PublishUrl=C:/WebAPI tvpapi_rest/WebAPI/WebAPI.csproj








FROM mcr.microsoft.com/windows/servercore/iis:${IIS_TAG}
SHELL ["powershell"]

RUN Install-WindowsFeature NET-Framework-45-ASPNET
RUN Install-WindowsFeature Web-Asp-Net45

COPY --from=builder WebAPI WebAPI

RUN Remove-WebSite -Name 'Default Web Site'; \
	New-Website -Name WebAPI -Port 80 -PhysicalPath 'C:\WebAPI'

RUN $filePath = \"C:\WINDOWS\System32\Inetsrv\Config\applicationHost.config\"; \
	$doc = New-Object System.Xml.XmlDocument; \
	$doc.Load($filePath); \
	$child = $doc.CreateElement(\"logFile\"); \
	$child.SetAttribute(\"directory\", \"C:\log\iis\%COMPUTERNAME%\"); \
	$site = $doc.SelectSingleNode(\"//site[@name='WebAPI']\"); \
	$site.AppendChild($child); \
	$doc.Save($filePath)
	
RUN mkdir C:\log
RUN mkdir C:\log\api

ARG API_LOG_DIR=C:\\log\\api\\%COMPUTERNAME%
ENV API_LOG_DIR ${API_LOG_DIR}

RUN mv /WebAPI/ssl-dev /Certificate

RUN import-module webadministration; \
    $pwd = ConvertTo-SecureString -String "123456" -Force -AsPlainText; \
    $cert = Import-PfxCertificate -Password $pwd -FilePath \"C:\\Certificate\\cert.pfx\" -CertStoreLocation \"Cert:\LocalMachine\My\"; \
    New-Item -path IIS:\SslBindings\0.0.0.0!443 -value $cert; \
    New-WebBinding -Name "WebAPI" -IP "*" -Port 443 -Protocol https


EXPOSE 80
EXPOSE 443

RUN $env:VERSION = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("C:\WebAPI\bin\WebAPI.dll").FileVersion
LABEL VERSION=${VERSION}
