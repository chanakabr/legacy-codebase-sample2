FROM node

RUN apt-get update
RUN apt-get install unzip

ENV TARGET_API ott
ENV NO_SSL true
ENV BASE_PATH /doc
ENV ENABLE_HOMEPAGE true
ENV KALTURA_RECIPES_PORT 80

ARG ZIP_URL=https://github.com/kaltura/developer-platform/archive/master.zip
ADD ${ZIP_URL} developer-platform.zip

RUN unzip developer-platform.zip
RUN rm -f developer-platform.zip
RUN apt-get remove -y unzip

RUN mkdir /opt/kaltura
RUN mv developer-platform-master/ /opt/kaltura/doc/
WORKDIR /opt/kaltura/doc
ADD generated generated/ott
RUN npm install

EXPOSE 80

CMD node server.js