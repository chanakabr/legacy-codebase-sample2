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

RUN Import-Module WebAdministration; \
	Set-ItemProperty 'IIS:\Sites\WebAPI' -Name logFile.enabled -Value True; \
	Set-ItemProperty 'IIS:\Sites\WebAPI' -Name logFile.directory -Value 'C:\log\iis\^%COMPUTERNAME^%'; \
	Set-ItemProperty 'IIS:\Sites\WebAPI' -Name logFile.logFormat W3C; \
	Set-ItemProperty 'IIS:\Sites\WebAPI' -Name logFile.period -Value MaxSize

RUN iisReset
	
RUN mklink /D C:\log\api\^%COMPUTERNAME^% C:\log\RestfulApi

COPY WebAPI WebAPI

EXPOSE 80