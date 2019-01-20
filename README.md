# Phoenix

## Deployment instructions:

1) Build documentation
	```
	docker build -t kaltura/phoenix-doc -f Dockerfile.Docs .
	```
	
2) Build sources
	```
	docker build -t kaltura/phoenix-build -f Dockerfile.Build --build-arg DOTNET_FRAMEWORK_TAG=4.7.2-sdk --build-arg BRANCH=master --build-arg BITBUCKET_TOKEN=<username>:<password> --build-arg GITHUB_TOKEN=<token> .
	```

3) Build Phoenix
	```
	docker build -t kaltura/phoenix-build -f Dockerfile.Phoenix --build-arg IIS_TAG=windowsservercore .
	```

3) Build Configuration Validator
	```
	docker build -t kaltura/phoenix-build -f Dockerfile.ConfigurationValidator --build-arg WINDOWS_TAG=ltsc2016 .
	```



Run docker
	```
	docker run --rm -it -p 80:80 -p 443:443 -e TCM_URL=http://172.31.25.52:8080 kaltura/phoenix
	```
