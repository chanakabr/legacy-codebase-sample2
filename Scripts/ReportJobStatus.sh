#!/bin/bash
branch=$1
stage=$2
buildnum=$3
jobname=$4
type=$5
status=$6

curl --silent -X POST \
  https://ux3dn9xfy3.execute-api.us-west-2.amazonaws.com/onebox/job/report \
  -H 'Content-Type: application/json' \
  -d '{
    "stage": "'$stage'",
    "branch": "'$branch'",
    "buildnumber": "'$buildnum'",
    "buildstatus": "'$status'",
    "job_name": "'$jobname'",
    "type": "'$type'"
}'
