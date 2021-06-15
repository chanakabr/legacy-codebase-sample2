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
setAllMigrationEventsStatusRequestUrl="http://$fronEndUrl/api_v3/service/canaryDeploymentConfiguration/action/setAllMigrationEventsStatus"
echo "sending canaryDeploymentConfiguration/action/setAllMigrationEventsStatus (set to true on groupId 0) request"
setAllMigrationEventsStatusResult=$(curl -s -X POST $setAllMigrationEventsStatusRequestUrl -H "Content-Type: application/json" -d '{ "groupId": 0, "ks": '"$systemAdminKs"', "status": true }' | jq .result)
if [ $setAllMigrationEventsStatusResult = true ]
then
	echo "set migration events status set to true on groupId 0 done successfully"	
else
	echo "Error - could not set migration events status to true on groupId 0"
	exit 1
fi
setAllRoutingActionsToMsRequestUrl="http://$fronEndUrl/api_v3/service/canaryDeploymentConfiguration/action/SetAllRoutingActionsToMs"
echo "sending canaryDeploymentConfiguration/action/SetAllRoutingActionsToMs (on groupId 0) request"
setAllRoutingActionsToMsResult=$(curl -s -X POST $setAllRoutingActionsToMsRequestUrl -H "Content-Type: application/json" -d '{ "groupId": 0, "ks": '"$systemAdminKs"' }' | jq .result)
if [ $setAllRoutingActionsToMsResult = true ]
then
	echo "set all routing actions to MS on groupId 0 done successfully"	
else
	echo "Error - could not set all routing actions to MS on groupId 0"
	exit 1
fi

