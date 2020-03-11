#!/bin/bash
branch=$1
stage=$2
buildnum=$3
jobname=$4
type=$5

curl --silent -X POST \
  https://ux3dn9xfy3.execute-api.us-west-2.amazonaws.com/onebox/job/update \
  -H 'Content-Type: application/json' \
  -d '{
    "branch": "'$branch'",
    "stage": "'$stage'",
    "buildnumber": "'$buildnum'",
    "buildstatus": "SUCCESS",
    "job_name": "'$jobname'",
    "type": "'$type'"
}'

