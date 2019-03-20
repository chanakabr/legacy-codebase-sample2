# TVP-API

## Deployment instructions:

1) Build docker
	```
	docker build -t kaltura/tvp-api --build-arg DOTNET_FRAMEWORK_TAG=4.7.2-sdk --build-arg BRANCH=master --build-arg BITBUCKET_TOKEN=<username>:<password> --build-arg IIS_TAG=windowsservercore .
	```
2) Run docker
	```
	docker run --rm -it -p 80:80 -p 443:443 kaltura/tvp-api
	```
