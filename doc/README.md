# Phoenix Docs

## Deployment instructions:

1) Build docker
	```
	docker build -t kaltura/phoenix-doc .
	```
2) Run docker
	```
	docker run --rm -it --name phoenix-doc -p 80:80 kaltura/phoenix-doc
	```
