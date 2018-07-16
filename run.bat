mkdir C:\log\iis\%COMPUTERNAME%
mklink /J C:\log\iis\%COMPUTERNAME%\%API_VERSION% C:\log\iisInternal
mklink /J C:\log\api\%COMPUTERNAME% C:\log\RestfulApi
C:\ServiceMonitor.exe w3svc
