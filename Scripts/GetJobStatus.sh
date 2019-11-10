#!/bin/bash
branch=$1
stage=$2
type=$3

curl -s -X POST \
https://ux3dn9xfy3.execute-api.us-west-2.amazonaws.com/onebox/job/status \
  -H 'Content-Type: application/json' \
  -d '{
  "branch": "'$branch'",
  "stage": "'$stage'",
  "type": "'$type'"
}' | jq '.[].buildstatus' | tr -d '""'


