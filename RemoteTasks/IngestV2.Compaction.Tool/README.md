#Ingest V2 Index Compaction Tool

This tool will proactivlly lunch index compaction logic, instead of waiting
for the IngestTransformation handler to run it during Ingest.

##Usage

* TCM values should point to same TCM as the IngestTransofrmationHandler
* The docker is a console app process that exits with code 0 if completed successfully, otherwise throws and error
* There is a single argument it expects for PartnerId as depicted in the example below
```bash

docker pull 870777418594.dkr.ecr.us-west-2.amazonaws.com/master/ott-tool-index-compaction:build

docker run -it --rm \
-e TCM_URL='http://tcm.service.consul:8443' \
-e TCM_APP='<IngestTransformationApp>' \
-e TCM_SECTION='<section>' \
-e TCM_HOST="<host>" \
-e TCM_APP_ID='5bf8cf60' \
-e TCM_APP_SECRET='5aaa99331c18f6bad4adeef93ab770c2' \
870777418594.dkr.ecr.us-west-2.amazonaws.com/master/ott-tool-index-compaction:build <partnerId>
```