FROM microsoft/iis
SHELL ["powershell"]

RUN Install-WindowsFeature NET-Framework-45-ASPNET
RUN Install-WindowsFeature Web-Asp-Net45

RUN Remove-WebSite -Name 'Default Web Site'
RUN MkDir WebAPI
RUN MkDir C:\Logs\IIS

RUN Import-Module WebAdministration; \
	cd IIS:\AppPools\; \
	$appPool = New-Item RestfulApi; \
	$appPool | Set-ItemProperty -Name managedRuntimeVersion -Value v4.0

RUN New-Website -Name WebAPI -Port 80 -PhysicalPath 'C:\WebAPI' -ApplicationPool RestfulApi

RUN Import-Module WebAdministration; \
	Set-ItemProperty 'IIS:\Sites\WebAPI' -Name logFile.enabled -Value True; \
	Set-ItemProperty 'IIS:\Sites\WebAPI' -Name logFile.directory -Value 'C:\Logs\IIS'; \
	Set-ItemProperty 'IIS:\Sites\WebAPI' -Name logFile.logFormat W3C; \
	Set-ItemProperty 'IIS:\Sites\WebAPI' -Name logFile.period -Value MaxSize

RUN iisReset
	
# filebeat should be downloaded from https://artifacts.elastic.co/downloads/beats/filebeat/filebeat-6.2.4-windows-x86_64.zip
ADD filebeat filebeat
RUN C:\filebeat\install-service-filebeat.ps1
RUN Start-Service filebeat

COPY WebAPI WebAPI

EXPOSE 80
