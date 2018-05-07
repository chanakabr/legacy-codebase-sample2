set DESTINATION=%1

powershell Invoke-WebRequest -OutFile filebeat.zip https://artifacts.elastic.co/downloads/beats/filebeat/filebeat-6.2.4-windows-x86_64.zip
powershell -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::ExtractToDirectory('filebeat.zip', 'filebeat-tmp'); }"

dir /B filebeat-tmp >tempfilelist.txt
mkdir %DESTINATION%\filebeat

for /f "tokens=1 delims=¬" %%b in (tempfilelist.txt) do (
    copy "filebeat-tmp\%%b" %DESTINATION%\filebeat\
    GOTO CLEANUP
)

:CLEANUP
del /q tempfilelist.txt
del /q filebeat.zip
rmdir /s /q filebeat-tmp