# TVP-API

## Deployment instructions:

1) Build docker
	```
	docker build --build-arg API_VERSION=v5_0_1 -t kaltura/tvp-api .
	```
2) Run docker
	```
	docker run --rm -it --name tvp-api -p 80:80 -v tvp_api_config:C:\WebAPI\Configuration kaltura/tvp-api
	```
