FROM microsoft/iis
SHELL ["powershell"]

RUN Install-WindowsFeature NET-Framework-45-ASPNET
RUN Install-WindowsFeature Web-Asp-Net45

RUN Remove-WebSite -Name 'Default Web Site'
RUN MkDir WebAPI
RUN MkDir log\iis
RUN MkDir log\iisInternal
RUN MkDir log\api
RUN MkDir log\RestfulApi

RUN Import-Module WebAdministration; \
	cd IIS:\AppPools\; \
	$appPool = New-Item RestfulApi; \
	$appPool | Set-ItemProperty -Name managedRuntimeVersion -Value v4.0

RUN New-Website -Name WebAPI -Port 80 -PhysicalPath 'C:\WebAPI' -ApplicationPool RestfulApi

RUN Import-Module WebAdministration; \
	Set-ItemProperty 'IIS:\Sites\WebAPI' -Name logFile.enabled -Value True; \
	Set-ItemProperty 'IIS:\Sites\WebAPI' -Name logFile.directory -Value 'C:\log\iisInternal'; \
	Set-ItemProperty 'IIS:\Sites\WebAPI' -Name logFile.logFormat W3C; \
	Set-ItemProperty 'IIS:\Sites\WebAPI' -Name logFile.period -Value MaxSize

RUN iisReset
	
COPY WebAPI WebAPI
COPY run.bat .

ARG API_VERSION
ENV API_VERSION ${API_VERSION}

ENTRYPOINT C:\run.bat

EXPOSE 80