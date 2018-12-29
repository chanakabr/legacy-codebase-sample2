ARG DOTNET_FRAMEWORK_TAG=4.7.2-sdk-windowsservercore-ltsc2019
ARG IIS_TAG=windowsservercore-ltsc2019
FROM microsoft/dotnet-framework:${DOTNET_FRAMEWORK_TAG} AS builder
SHELL ["powershell"]

ARG BRANCH=master
ARG BITBUCKET_TOKEN
ARG GITHUB_TOKEN

ADD https://${BITBUCKET_TOKEN}@bitbucket.org/tvinci_dev/tvincicommon/get/${BRANCH}.zip tvincicommon.zip
ADD https://${BITBUCKET_TOKEN}@bitbucket.org/tvinci_dev/tvplibs/get/${BRANCH}.zip tvplibs.zip

RUN Expand-Archive tvincicommon.zip -DestinationPath tmp; mv tmp/$((Get-ChildItem tmp | Select-Object -First 1).Name) tvincicommon; rm tmp; rm tvincicommon.zip
RUN Expand-Archive tvplibs.zip -DestinationPath tmp; mv tmp/$((Get-ChildItem tmp | Select-Object -First 1).Name) tvplibs; rm tmp; rm tvplibs.zip

ADD . tvpapi

RUN nuget restore tvpapi/TVPProAPIs.sln


RUN msbuild /p:Configuration=Release tvpapi/TVPProAPIs.sln
RUN msbuild /t:WebPublish /p:Configuration=Release /p:DeployOnBuild=True /p:WebPublishMethod=FileSystem /p:PublishUrl=C:/WebAPI tvpapi/WS_TVPApi/website.publishproj










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
	
ARG API_LOG_DIR=C:\\log\\api\\%COMPUTERNAME%
ENV API_LOG_DIR ${API_LOG_DIR}

RUN mv WebAPI\\ssl-dev C:\\Certificate

RUN import-module webadministration; \
    $pwd = ConvertTo-SecureString -String "123456" -Force -AsPlainText; \
    $cert = Import-PfxCertificate -Password $pwd -FilePath \"C:\\Certificate\\cert.pfx\" -CertStoreLocation \"Cert:\LocalMachine\My\"; \
    New-Item -path IIS:\SslBindings\0.0.0.0!443 -value $cert; \
    New-WebBinding -Name "WebAPI" -IP "*" -Port 443 -Protocol https

EXPOSE 80
EXPOSE 443