#!/bin/bash
branch=$1
stage=$2

curl -s -X POST \
https://ux3dn9xfy3.execute-api.us-west-2.amazonaws.com/onebox/job/stage/status \
  -H 'Content-Type: application/json' \
  -d '{
  "branch": "'$branch'",
  "stage": "'$stage'"
}' 
#| jq '.[].buildstatus' | tr -d '""'

