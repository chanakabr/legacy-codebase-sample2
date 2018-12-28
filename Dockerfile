ARG DOTNET_FRAMEWORK_TAG=4.7.2-sdk-windowsservercore-ltsc2019
ARG IIS_TAG=windowsservercore-ltsc2019
FROM microsoft/dotnet-framework:${DOTNET_FRAMEWORK_TAG} AS builder
SHELL ["powershell"]

ARG BRANCH=master
ARG BITBUCKET_TOKEN
ARG GITHUB_TOKEN

ADD https://${BITBUCKET_TOKEN}@bitbucket.org/tvinci_dev/tvmcore/get/${BRANCH}.zip TvmCore.zip
ADD https://${BITBUCKET_TOKEN}@bitbucket.org/tvinci_dev/tvplibs/get/${BRANCH}.zip tvplibs.zip
ADD https://${BITBUCKET_TOKEN}@bitbucket.org/tvinci_dev/tvincicommon/get/${BRANCH}.zip tvincicommon.zip
ADD https://${BITBUCKET_TOKEN}@bitbucket.org/tvinci_dev/CDNTokenizers/get/${BRANCH}.zip CDNTokenizers.zip
ADD https://api.github.com/repos/kaltura/Core/zipball/${BRANCH}?access_token=${GITHUB_TOKEN} Core.zip
ADD https://${BITBUCKET_TOKEN}@bitbucket.org/tvinci_dev/tvpapi_rest/get/${BRANCH}.zip tvpapi_rest.zip

RUN Expand-Archive TvmCore.zip -DestinationPath tmp; mv tmp/$((Get-ChildItem tmp | Select-Object -First 1).Name) TvmCore; rm tmp; rm TvmCore.zip
RUN Expand-Archive tvplibs.zip -DestinationPath tmp; mv tmp/$((Get-ChildItem tmp | Select-Object -First 1).Name) tvplibs; rm tmp; rm tvplibs.zip
RUN Expand-Archive tvincicommon.zip -DestinationPath tmp; mv tmp/$((Get-ChildItem tmp | Select-Object -First 1).Name) tvincicommon; rm tmp; rm tvincicommon.zip
RUN Expand-Archive CDNTokenizers.zip -DestinationPath tmp; mv tmp/$((Get-ChildItem tmp | Select-Object -First 1).Name) CDNTokenizers; rm tmp; rm CDNTokenizers.zip
RUN Expand-Archive Core.zip -DestinationPath tmp; mv tmp/$((Get-ChildItem tmp | Select-Object -First 1).Name) Core; rm tmp; rm Core.zip
RUN Expand-Archive tvpapi_rest.zip -DestinationPath tmp; mv tmp/$((Get-ChildItem tmp | Select-Object -First 1).Name) tvpapi_rest; rm tmp; rm tvpapi_rest.zip

ADD . RemoteTasks

RUN nuget restore TvmCore/TvinciCore.sln
RUN nuget restore RemoteTasks/RemoteTasksService.sln
RUN nuget restore tvpapi_rest/TVPProAPIs.sln
RUN nuget restore Core/Core.sln

RUN msbuild /p:Configuration=Release RemoteTasks/RemoteTasksService.sln
RUN msbuild /t:WebPublish /p:Configuration=Release /p:DeployOnBuild=True /p:WebPublishMethod=FileSystem /p:PublishUrl=C:/RemoteTasksService RemoteTasks/RemoteTasksService/RemoteTasksService.csproj










FROM mcr.microsoft.com/windows/servercore/iis:${IIS_TAG}
SHELL ["powershell"]

RUN Install-WindowsFeature NET-Framework-45-ASPNET
RUN Install-WindowsFeature Web-Asp-Net45
RUN Add-WindowsFeature NET-WCF-HTTP-Activation45

COPY --from=builder RemoteTasksService RemoteTasksService

RUN Remove-WebSite -Name 'Default Web Site'; \
	New-Website -Name RemoteTasks -Port 80 -PhysicalPath 'C:\RemoteTasksService'
	
RUN $filePath = \"C:\WINDOWS\System32\Inetsrv\Config\applicationHost.config\"; \
	$doc = New-Object System.Xml.XmlDocument; \
	$doc.Load($filePath); \
	$child = $doc.CreateElement(\"logFile\"); \
	$child.SetAttribute(\"directory\", \"C:\log\iis\%COMPUTERNAME%\"); \
	$site = $doc.SelectSingleNode(\"//site[@name='RemoteTasks']\"); \
	$site.AppendChild($child); \
	$doc.Save($filePath)
	
ARG REMOTE_TASK_LOG_DIR=C:\\log\\remotetask\\%COMPUTERNAME%
ENV REMOTE_TASK_LOG_DIR ${REMOTE_TASK_LOG_DIR}

RUN mv RemoteTasksService\\ssl-dev C:\\Certificate

RUN import-module webadministration; \
    $pwd = ConvertTo-SecureString -String "123456" -Force -AsPlainText; \
    $cert = Import-PfxCertificate -Password $pwd -FilePath \"C:\\Certificate\\cert.pfx\" -CertStoreLocation \"Cert:\LocalMachine\My\"; \
    New-Item -path IIS:\SslBindings\0.0.0.0!443 -value $cert; \
    New-WebBinding -Name "RemoteTasks" -IP "*" -Port 443 -Protocol https

EXPOSE 80
EXPOSE 443