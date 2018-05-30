FROM microsoft/iis
SHELL ["powershell"]

RUN Install-WindowsFeature NET-Framework-45-ASPNET
RUN Install-WindowsFeature Web-Asp-Net45

RUN Remove-WebSite -Name 'Default Web Site'
RUN MkDir RemoteTasksService

RUN Import-Module WebAdministration; \
	cd IIS:\AppPools\; \
	$appPool = New-Item RemoteTasks; \
	$appPool | Set-ItemProperty -Name managedRuntimeVersion -Value v4.0

RUN New-Website -Name RemoteTasks -Port 80 -PhysicalPath 'C:\RemoteTasksService' -ApplicationPool RemoteTasks

RUN Import-Module WebAdministration; \
	Set-ItemProperty 'IIS:\Sites\RemoteTasks' -Name logFile.enabled -Value True; \
	Set-ItemProperty 'IIS:\Sites\RemoteTasks' -Name logFile.directory -Value 'C:\log\iis'; \
	Set-ItemProperty 'IIS:\Sites\RemoteTasks' -Name logFile.logFormat W3C; \
	Set-ItemProperty 'IIS:\Sites\RemoteTasks' -Name logFile.period -Value MaxSize

RUN iisReset
	
COPY RemoteTasksService RemoteTasksService

EXPOSE 80