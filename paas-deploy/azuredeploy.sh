#!/bin/bash
rg='multipleapis'
location='centralus'
tenantName='blueskyabove.onmicrosoft.com'

#Create AAD application registration for web API application
tenantName="${tenantName,,}"
apiAppIdUrl=https://$tenantName/aad-dotnet-webapi-onbehalfof
apiDisplayName='aad-dotnet-webapi-onbehalfof'
apiClientSecret=$(openssl rand -base64 32)
az ad app create --display-name $apiDisplayName --homepage https://localhost:44330/ --identifier-uris $apiAppIdUrl --password $apiClientSecret
apiAppId=$(az ad app show --id $apiAppIdUrl --query "appId" --output tsv)
az ad sp create --id $apiAppId

#Create AAD application registration for web application
appIdUrl=https://$tenantName/aad-dotnet-multiple-apis
displayName='aad-dotnet-multiple-apis'
clientSecret=$(openssl rand -base64 32)
az ad app create --display-name $displayName --homepage https://localhost:44320/ --identifier-uris $appIdUrl --password $clientSecret
appId=$(az ad app show --id $appIdUrl --query "appId" --output tsv)

#Generate Azure SQL Database admin password
sqlAdminLogin='myAdmin'
sqlPassword=$(openssl rand -base64 32)

#Create the resource group
az group create --name $rg --location $location

#Get user and tenant information
userUPN=$(az account show --query "user.name" --o tsv)
userObjectId=$(az ad user show --upn-or-object-id $userUPN --query "objectId" --o tsv)
tenantId=$(az account show --query "tenantId" --o tsv)
subscriptionId=$(az account show --query "id" --o tsv)

#Generate a random GUID for the role assignment ID
roleAssignmentGuid=$(cat /proc/sys/kernel/random/uuid)

#Assign the user to the Storage Blob Data Contributor role
roleDefinitionId=$(az role definition list --query "[?roleName == 'Storage Blob Data Contributor (Preview)'].id" --output tsv)
az role assignment create --assignee $userObjectId --role $roleDefinitionId --resource-group $rg

az group deployment create \
  --name "multiple-apis-deployment" \
  --resource-group $rg \
  --template-file "azuredeploy.json" \
  --parameters sqlAdminLogin=$sqlAdminLogin sqlAdminPassword=$sqlPassword aadUserUPN=$userUPN aadUserObjectID=$userObjectId clientId=$appId apiClientId=$apiAppId tenant=$tenantName

#Add the client secrets to the newly created vault
vaultname=$(az keyvault list --resource-group $rg --query "[0].name" --output tsv)
az keyvault secret set --vault-name $vaultname --name 'multiple-apis-client-secret' --value $clientSecret
az keyvault secret set --vault-name $vaultname --name 'webapi-onbehalfof-client-secret' --value $apiClientSecret

#Add the web apps' URLs as a reply URL to each registered AAD application.
#Update the client secret in case the app already existed and a new password was generated.
webapp=$(az group deployment show -g $rg -n 'multiple-apis-deployment' --query properties.outputs.appUrl.value --output tsv)
az ad app update --id $appId --password $clientSecret --reply-urls $webapp https://localhost:44320/

webApi=$(az group deployment show -g $rg -n 'multiple-apis-deployment' --query properties.outputs.apiAppUrl.value --output tsv)
az ad app update --id $apiAppId --password $apiClientSecret --reply-urls $webApi https://localhost:44330/

#Add permission from web application to web API
permissionId=$(az ad app show --id $apiAppId --query "oauth2Permissions[0].id" --output tsv)
az ad app permission add --id $appId --api $apiAppId --api-permissions $permissionId=Scope

#Add permissions from web application to AAD Graph, Microsoft Graph, Storage, Database, and ARM 
az ad app permission add --id $appId --api 00000003-0000-0000-c000-000000000000 --api-permissions b340eb25-3456-403f-be2f-af7a0d370277=Scope e1fe6dd8-ba31-4d61-89e7-88639da4683d=Scope
az ad app permission add --id $appId --api 00000002-0000-0000-c000-000000000000 --api-permissions 311a71cc-e848-46a1-bdf8-97ff7156d8e6=Scope
az ad app permission add --id $appId --api 797f4846-ba00-4fd7-ba43-dac1f8f63013 --api-permissions 41094075-9dad-400e-a0bd-54e686782033=Scope
az ad app permission add --id $appId --api 022907d3-0f1b-48f7-badc-1ba6abab6d66 --api-permissions c39ef2d1-04ce-46dc-8b5f-e9a5c60f0fc9=Scope
az ad app permission add --id $appId --api e406a681-f3d4-42a8-90b6-c2b029497af1 --api-permissions 03e0da56-190b-40ad-a80c-ea378c433f7f=Scope

#Add permissions from API application to AAD Graph, Microsoft Graph
az ad app permission add --id $apiAppId --api 00000003-0000-0000-c000-000000000000 --api-permissions b340eb25-3456-403f-be2f-af7a0d370277=Scope e1fe6dd8-ba31-4d61-89e7-88639da4683d=Scope
az ad app permission add --id $apiAppId --api 00000002-0000-0000-c000-000000000000 --api-permissions 311a71cc-e848-46a1-bdf8-97ff7156d8e6=Scope

