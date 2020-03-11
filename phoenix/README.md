# Phoenix

## Deployment instructions:
	
1) Build Core
	```
	cd ..\Core
	docker build -t kaltura/core:dev --build-arg DOTNET_FRAMEWORK_TAG=4.7.2-sdk --build-arg BRANCH=master --build-arg BITBUCKET_TOKEN=<username>:<password> --build-arg API_VERSION=v5_2_0 .
	docker tag kaltura/core:dev 870777418594.dkr.ecr.eu-west-1.amazonaws.com/core:dev
	docker push 870777418594.dkr.ecr.eu-west-1.amazonaws.com/core:dev
	cd ..\tvpapi_rest
	```

2) Build Phoenix
	```
	docker build -t kaltura/phoenix:dev --build-arg BUILD_TAG=dev --build-arg IIS_TAG=windowsservercore .
	docker tag kaltura/phoenix:dev 870777418594.dkr.ecr.eu-west-1.amazonaws.com/dev-phoenix:dev
	docker push 870777418594.dkr.ecr.eu-west-1.amazonaws.com/dev-phoenix:dev
	```

3) Build Web-Services
	```
	docker build -t kaltura/web-services -f Dockerfile.WebServices --build-arg BUILD_TAG=dev --build-arg IIS_TAG=windowsservercore .
	```

4) Build documentation
	```
	docker build -t kaltura/phoenix-doc -f Dockerfile.Docs --build-arg BASE_PATH=/doc/ --build-arg VERSION=5.2.3.0 --build-arg API_VERSION=5.2.3 --build-arg ENABLE_HOMEPAGE=false .
	```
	```
	docker run --rm -it -p 80:80 kaltura/phoenix-doc
	```

5) Build Configuration Validator
	```
	docker build -t kaltura/phoenixconfiguration-validator -f Dockerfile.ConfigurationValidator --build-arg WINDOWS_TAG=ltsc2016 .
	```



Run docker
	```
	docker run --rm -it -p 80:80 -p 443:443 -e TCM_URL=http://172.31.25.52:8080 kaltura/phoenix
	```
