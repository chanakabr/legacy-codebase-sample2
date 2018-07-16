# Phoenix

## Deployment instructions:

1) Build docker
	```
	docker build --build-arg API_VERSION=v5_0_1 -t kaltura/phoenix .
	```
2) Run docker
	```
	docker run --rm --name phoenix -p 80:80 kaltura/phoenix
	```
