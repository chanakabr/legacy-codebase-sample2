mkdir C:\log\iis\%COMPUTERNAME%
mklink /D C:\log\iis\%COMPUTERNAME%\%API_VERSION% C:\log\iisInternal
mklink /D C:\log\api\%COMPUTERNAME% C:\log\RestfulApi
C:\ServiceMonitor.exe w3svc
