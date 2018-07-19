FROM microsoft/iis
SHELL ["powershell"]

RUN Install-WindowsFeature NET-Framework-45-ASPNET
RUN Install-WindowsFeature Web-Asp-Net45
RUN Add-WindowsFeature NET-WCF-HTTP-Activation45

RUN MkDir Ingest

RUN Remove-WebSite -Name 'Default Web Site'; \
	New-Website -Name Ingest -Port 80 -PhysicalPath 'C:\Ingest'
	
RUN $filePath = \"C:\WINDOWS\System32\Inetsrv\Config\applicationHost.config\"; \
	$doc = New-Object System.Xml.XmlDocument; \
	$doc.Load($filePath); \
	$child = $doc.CreateElement(\"logFile\"); \
	$child.SetAttribute(\"directory\", \"C:\log\iis\%COMPUTERNAME%\"); \
	$site = $doc.SelectSingleNode(\"//site[@name='WebAPI']\"); \
	$site.AppendChild($child); \
	$doc.Save($filePath)
	
ARG INGEST_LOG_DIR=C:\\log\\ingest\\%COMPUTERNAME%
ENV INGEST_LOG_DIR ${INGEST_LOG_DIR}

COPY Ingest Ingest

EXPOSE 80