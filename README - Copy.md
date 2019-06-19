# OTT Core


## Deployment instructions:

1) Build documentation
	```
	docker build --rm=false -t kaltura/core:dev --build-arg DOTNET_FRAMEWORK_TAG=4.7.2-sdk --build-arg BRANCH=master --build-arg BITBUCKET_TOKEN=<username>:<password> --build-arg API_VERSION=v5_2_0 .
	docker tag kaltura/core:dev 870777418594.dkr.ecr.eu-west-1.amazonaws.com/core:dev
	docker push 870777418594.dkr.ecr.eu-west-1.amazonaws.com/core:dev
	```
	