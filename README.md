# Phoenix

## Deployment instructions:

1) Build docker
	```
	docker build -t kaltura/ingest --build-arg DOTNET_FRAMEWORK_TAG=4.7.2-sdk --build-arg BRANCH=master --build-arg BITBUCKET_TOKEN=<username>:<password> --build-arg GITHUB_TOKEN=<token> --build-arg IIS_TAG=windowsservercore .
	```
2) Run docker
	```
	docker run --rm -it -p 80:80 -p 443:443 -e TCM_URL=http://172.31.25.52:8080 kaltura/ingest
	```
