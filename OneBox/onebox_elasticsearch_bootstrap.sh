#!/bin/bash
# Comment\ Uncomment this for trace output
# set -o xtrace
fronEndUrl=$1
loginRequestUrl="http://$fronEndUrl/api_v3/service/ottuser/action/login"
echo "sending system administrator login request"
systemAdminKs=$(curl -s -X POST $loginRequestUrl -H "Content-Type: application/json" -d '{ "partnerId": 1483, "username": "systemAdmin_1483", "password": "oneboxMagic" }' | jq .result.loginSession.ks)
if [ $systemAdminKs = null ]
then
      echo "Error - could not get system administrator KS"
	  exit 1
else
      echo "system administrator KS fetched successfully "
fi
setElasticActiveVersionRequestUrl="http://$fronEndUrl/api_v3/service/elasticsearchCanaryDeploymentConfiguration/action/SetActiveVersion"
echo "sending elasticsearchCanaryDeploymentConfiguration/action/SetActiveVersion (on groupId 0) request"
setAllRoutingActionsToMsResult=$(curl -s -X POST $setElasticActiveVersionRequestUrl -H "Content-Type: application/json" -d '{ "groupId": 0, "ks": '"$systemAdminKs"', "activeVersion": "ES_7" }' | jq .result)
if [ $setAllRoutingActionsToMsResult = true ]
then
	echo "set elasticsearch active version on groupId 0 done successfully"	
else
	echo "Error - could not set elasticsearch active version on groupId 0"
	exit 1
fi

