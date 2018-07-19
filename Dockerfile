FROM microsoft/iis
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

RUN Install-WindowsFeature NET-Framework-45-ASPNET
RUN Install-WindowsFeature Web-Asp-Net45
RUN Add-WindowsFeature NET-WCF-HTTP-Activation45

RUN MkDir RemoteTasksService

RUN Remove-WebSite -Name 'Default Web Site'; \
	New-Website -Name RemoteTasks -Port 80 -PhysicalPath 'C:\RemoteTasksService' -ApplicationPool RemoteTasks
	
RUN $filePath = \"C:\WINDOWS\System32\Inetsrv\Config\applicationHost.config\"; \
	$doc = New-Object System.Xml.XmlDocument; \
	$doc.Load($filePath); \
	$child = $doc.CreateElement(\"logFile\"); \
	$child.SetAttribute(\"directory\", \"C:\log\iis\%COMPUTERNAME%\"); \
	$site = $doc.SelectSingleNode(\"//site[@name='RemoteTasks']\"); \
	$site.AppendChild($child); \
	$doc.Save($filePath)
	
ARG REMOTE_TASK_LOG_DIR=C:\\log\\ingest\\%COMPUTERNAME%
ENV REMOTE_TASK_LOG_DIR ${REMOTE_TASK_LOG_DIR}

COPY RemoteTasksService RemoteTasksService

EXPOSE 80