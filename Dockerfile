FROM microsoft/iis
SHELL ["powershell"]

RUN Install-WindowsFeature NET-Framework-45-ASPNET
RUN Install-WindowsFeature Web-Asp-Net45

RUN MkDir WebAPI

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

COPY WS_TVPApi WebAPI

EXPOSE 80