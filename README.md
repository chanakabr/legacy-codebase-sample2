# Phoenix

## Deployment instructions:

1) Build docker
	```
	docker build -t kaltura/remotetasks .
	```
2) Run docker
	```
	docker run -d ^
		-p 80:80 ^
		-v iis_log:C:\log\iis ^
		-v remote_tasks_log:C:\log\remote_tasks ^
		kaltura/remotetasks
	```
