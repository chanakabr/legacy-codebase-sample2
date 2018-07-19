# Ingest

## Deployment instructions:

1) Build docker
	```
	docker build -t kaltura/ingest .
	```
2) Run docker
	```
	docker run -d ^
		-p 80:80 ^
		-v iis_log:C:\log\iis ^
		-v ingest_log:C:\log\ingest ^
		kaltura/ingest
	```
