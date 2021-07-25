docker network create elastic7network
docker run -d --name elasticsearch --net elastic7network -p 9200:9200 -p 9300:9300 -e "discovery.type=single-node" elasticsearch:7.13.4