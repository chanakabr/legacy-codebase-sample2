FROM mcr.microsoft.com/windows/servercore/iis:windowsservercore-ltsc2019
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

RUN Install-WindowsFeature NET-Framework-45-ASPNET
RUN Install-WindowsFeature Web-Asp-Net45
RUN Add-WindowsFeature NET-WCF-HTTP-Activation45

RUN MkDir RemoteTasksService

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

COPY . RemoteTasksService
COPY ssl-dev\\cert.pfx C:\\Certificate\\cert.pfx

RUN import-module webadministration; \
    $pwd = ConvertTo-SecureString -String "123456" -Force -AsPlainText; \
    $cert = Import-PfxCertificate -Password $pwd -FilePath \"C:\\Certificate\\cert.pfx\" -CertStoreLocation \"Cert:\LocalMachine\My\"; \
    New-Item -path IIS:\SslBindings\0.0.0.0!443 -value $cert; \
    New-WebBinding -Name "RemoteTasks" -IP "*" -Port 443 -Protocol https

EXPOSE 80
EXPOSE 443

ARG VERSION
LABEL version=${VERSION}