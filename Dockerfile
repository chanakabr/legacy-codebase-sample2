FROM microsoft/iis
SHELL ["powershell"]

RUN Install-WindowsFeature NET-Framework-45-ASPNET
RUN Install-WindowsFeature Web-Asp-Net45

RUN Remove-WebSite -Name 'Default Web Site'
RUN MkDir WebAPI

RUN Import-Module WebAdministration; \
	cd IIS:\AppPools\; \
	$appPool = New-Item RestfulApi; \
	$appPool | Set-ItemProperty -Name managedRuntimeVersion -Value v4.0

RUN New-Website -Name WebAPI -Port 80 -PhysicalPath 'C:\WebAPI' -ApplicationPool RestfulApi

RUN $filePath = "$Env:WinDir\System32\Inetsrv\Config"; \
	$doc = New-Object System.Xml.XmlDocument; \
	$doc.Load($filePath); \
	$child = $doc.CreateElement("logFile"); \
	$child.SetAttribute("logExtFileFlags", "Date, Time, ClientIP, UserName, ComputerName, ServerIP, Method, UriStem, UriQuery, HttpStatus, Win32Status, TimeTaken, ServerPort, UserAgent, Referer, HttpSubStatus"); \
	$child.SetAttribute("directory", "%IIS_LOG_DIR%"); \
	$site = $doc.SelectSingleNode("//site[@name='WebAPI']"); \
	$site.AppendChild($child); \
	$doc.Save($filePath)
	
ARG API_LOG_DIR=C:\log\api\%COMPUTERNAME%
ENV API_LOG_DIR ${API_LOG_DIR}

ARG IIS_LOG_DIR=C:\log\iis\%COMPUTERNAME%
ENV IIS_LOG_DIR ${IIS_LOG_DIR}

RUN iisReset
	
COPY WebAPI WebAPI

EXPOSE 80