#!/bin/bash
rg='multipleapis'
location='centralus'
appIdUrl='https://multipleapis'
tenantName='blueskyabove.onmicrosoft.com'
displayName='multipleapis'

#Create AAD application registration
clientSecret=$(openssl rand -base64 32)
az ad app create --display-name $displayName --homepage https://localhost:44320/ --identifier-uris $appIdUrl --required-resource-accesses manifest.json --password $clientSecret
appId=$(az ad app show --id $appIdUrl --query "appId" --output tsv)

#Get user and tenant information
userUPN=$(az account show --query "user.name" --o tsv)
userObjectId=$(az ad user show --upn-or-object-id $userUPN --query "objectId" --o tsv)
tenantId=$(az account show --query "tenantId" --o tsv)

#Generate Azure SQL Database admin password
sqlAdminLogin='myAdmin'
sqlPassword=$(openssl rand -base64 32)

#Generate a random GUID for the role assignment ID
roleAssignmentGuid=$(cat /proc/sys/kernel/random/uuid)

az group create --name $rg --location $location

az group deployment create \
  --name "multiple-apis-deployment" \
  --resource-group $rg \
  --template-file "azuredeploy.json" \
  --parameters hostingPlanName=$displayName sqlAdminLogin=$sqlAdminLogin sqlAdminPassword=$sqlPassword databaseName=advworks aadUserUPN=$userUPN aadUserObjectID=$userObjectId clientId=$appId tenant=$tenantName roleAssignmentGuid=$roleAssignmentGuid

#Add the app's client secret to the newly created vault
vaultname=$(az keyvault list --resource-group $rg --query "[0].name" --output tsv)
az keyvault secret set --vault-name $vaultname --name 'multiple-apis-client-secret' --value $clientSecret

#Add the web app's URL as a reply URL to the registered AAD application.
#Update the client secret in case the app already existed and a new password was generated.
webapp=https://$(az webapp list --resource-group $rg --query "[0].defaultHostName" --output tsv)
az ad app update --id $appId --password $clientSecret --reply-urls $webapp https://localhost:44320/