FROM microsoft/iis:windowsservercore-1803
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

COPY WebAPI WebAPI
COPY WebAPI\\ssl-dev\\cert.pfx C:\\Certificate\\cert.pfx

RUN $pwd = ConvertTo-SecureString -String "123456" -Force -AsPlainText; \
    $cert = Import-PfxCertificate -Password $pwd -FilePath \"C:\\Certificate\\cert.pfx\" -CertStoreLocation \"Cert:\LocalMachine\My\"; \
    $guid = [guid]::NewGuid().ToString(\"B\"); \
    netsh http add sslcert hostnameport=\"*:443\" certhash=$cert certstorename=MY appid=\"$guid\"; \
    New-WebBinding -name WebApi -Protocol https -Port 443 -SslFlags 0



EXPOSE 80
EXPOSE 443

ARG VERSION
LABEL version=${VERSION}