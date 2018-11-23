# aad-dotnet-multiple-apis

Example of using multiple APIs with ADAL to Azure-protected resources.

## Overview of solution

The solution is an ASP.NET MVC web app that authenticates the user using OpenId Connect with the ADAL library and Azure AD. The app is deployed to an Azure App Service that has been assigned a Managed Service Identity (MSI). This MSI is given permission to an Azure Key Vault where the AAD app's clientSecret is stored, avoiding storing any secrets in configuration. Once the user logs in, you can choose from various APIs in the menu to test accessing various services such as Azure Management API, Azure Storage, Azure SQL Database, Azure AD Graph API, and Microsoft Graph API. The access tokens for the application are stored in an Azure SQL Database.

## Deploying the template

To deploy the solution:

- Go to the Azure Portal and **open** the Azure Cloud Shell using Bash.
- Use the upload button to **upload** `manifest.json` and `azuredeploy.json` from the paas-deploy project.
- Run the following commands, providing your own values for the parameters.

```bash
#!/bin/bash
rg='multiple-apis'
location='centralus'
appIdUrl='https://manyapitest'
tenantName='blueskyabove.onmicrosoft.com'
displayName='manyapisdemo'

#Create AAD application registration
clientSecret=$(openssl rand -base64 32)
az ad app create --display-name $displayName --homepage $appIdUrl --identifier-uris $appIdUrl --required-resource-accesses manifest.json --password $clientSecret
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

#Add the web app's URL as a reply URL to the registered AAD application
webapp=https://$(az webapp list --resource-group $rg --query "[0].defaultHostName" --output tsv)
az ad app update --id $appId --reply-urls $webapp
```

The same script (`azuredeploy.sh`) is available in the paas-deploy project.

## Deployment parameters description

The following table describes the various parameters used.

Property Name | Description | Sample Value
--- | --- | ---
hostingPlanName | Name of the App Service plan | kirkeplan
sqlAdminLogin | The administrator login for the Azure SQL server | myadmin
sqlAdminPassword | The administrator password for the Azure SQL server | somepassword
databaseName | The name of the Azure SQL Database | advworks
aadUserUPN | Your user principal name in Azure AD | kirke@microsoft.com
aadUserObjectID | The object ID of your user object in Azure AD | 45782e73-012e-4ef3-9ecb-560157c8e927
clientID | The `appId` of the newly created app registration created prior to deployment | 45782e73-012e-4ef3-9ecb-560157c8e927
tenant | The Azure AD tenant name | blueskyabove.onmicrosoft.com
roleAssignmentGuid | A guid that uniquely identifies the assignment of a user to a role. | b1b9fffc-112e-4d14-b0d5-611b16222c05

## Running the solution locally
If you want to run the solution locally, update the web.config file with the `ida:ClientSecret` value for your app registration and set the `deployType` appSetting value to `local`.

## Troubleshooting
The deployment from GitHub to the web app occasionally fails. Go to the Azure Portal, find the web app, go to the Deployment Center, and re-deploy the app. And if you figure out why it doesn't consistently deploy, feel free to submit a pull request.