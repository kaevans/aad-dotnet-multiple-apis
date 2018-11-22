# aad-dotnet-multiple-apis
Example of using multiple APIs with ADAL to Azure-protected resources

## Pre-deployment
Before deployment, use the Azure CLI to create a new app registration and a client secret. 

Create a JSON file named `manifest.json` with the following contents:

````json
[
    {
      "resourceAppId": "00000003-0000-0000-c000-000000000000",
      "resourceAccess": [
        {
          "id": "b340eb25-3456-403f-be2f-af7a0d370277",
          "type": "Scope"
        },
        {
          "id": "e1fe6dd8-ba31-4d61-89e7-88639da4683d",
          "type": "Scope"
        },
        {
          "id": "7427e0e9-2fba-42fe-b0c0-848c9e6a8182",
          "type": "Scope"
        },
        {
          "id": "14dad69e-099b-42c9-810b-d002981feec1",
          "type": "Scope"
        },
        {
          "id": "64a6cdd6-aab1-4aaf-94b8-3cc8405e90d0",
          "type": "Scope"
        },
        {
          "id": "37f7f235-527c-4136-accd-4a02d197296e",
          "type": "Scope"
        },
        {
          "id": "b4e74841-8e56-480b-be8b-910348b18b4c",
          "type": "Scope"
        }
      ]
    },
    {
      "resourceAppId": "e406a681-f3d4-42a8-90b6-c2b029497af1",
      "resourceAccess": [
        {
          "id": "03e0da56-190b-40ad-a80c-ea378c433f7f",
          "type": "Scope"
        }
      ]
    },
    {
      "resourceAppId": "022907d3-0f1b-48f7-badc-1ba6abab6d66",
      "resourceAccess": [
        {
          "id": "c39ef2d1-04ce-46dc-8b5f-e9a5c60f0fc9",
          "type": "Scope"
        }
      ]
    },
    {
      "resourceAppId": "797f4846-ba00-4fd7-ba43-dac1f8f63013",
      "resourceAccess": [
        {
          "id": "41094075-9dad-400e-a0bd-54e686782033",
          "type": "Scope"
        }
      ]
    },
    {
      "resourceAppId": "00000002-0000-0000-c000-000000000000",
      "resourceAccess": [
        {
          "id": "311a71cc-e848-46a1-bdf8-97ff7156d8e6",
          "type": "Scope"
        }
      ]
    }
  ]
````
Once the manifest.json file is created, create a new app registration referencing this file. Grab the user's object ID and the AAD tenant ID

````bash
clientSecret=$(openssl rand -base64 32)
az ad app create --display-name cliapptest --homepage http://manyapps --identifier-uris https://manyapps --required-resource-accesses manifest.json --password $clientSecret
appId=$(az ad app show --id https://manyapps --query "appId")

userObectId=$(az ad user show --upn-or-object-id kirkevans@blueskyabove.onmicrosoft.com --query "objectId")
tenantId=$(az account show --subscription msdn --query "tenantId")

sqlAdminPassword=$(openssl rand -base64 32)

roleAssignmentGuid=$(cat /proc/sys/kernel/random/uuid)
````
This script created the app registration and stored the resulting value in a variable `appId` that will be used in the next step.


## Deploying the tempalte

An example script with sample values is shown. Replace the parameter values with your own.

````bash
az group create --name 'multiple-apis' --location centralus
az group deployment create \
  --name "multiple-apis-deployment" \
  --resource-group "multiple-apis" \
  --template-file "azuredeploy.json" \
  --parameters hostingPlanName=kirkeplan \
      administratorLogin=myAdmin \
      databaseName=advworks \
      administratorLoginPassword=$sqlAdminPassword \
      aadAdminUPN="kirke@microsoft.com" \
      aadAdminObjectID=$userObectId \
      clientId=$appId \
      tenant=blueskyabove.onmicrosoft.com \
      roleAssignmentGuid=$roleAssignmentGuid
````
The following table describes the various parameters used.

Property Name | Description | Sample Value
--- | --- | ---
hostingPlanName | Name of the App Service plan | kirkeplan
administratorLogin | The administrator login for the Azure SQL server | myadmin
administratorLoginPassword | The administrator password for the Azure SQL server | somepassword
databaseName | The name of the Azure SQL Database | advworks
aadAdminUPN | Your user principal name in Azure AD | kirke@microsoft.com
aadAdminObjectID | The object ID of your user object in Azure AD | 45782e73-012e-4ef3-9ecb-560157c8e927
clientID | The `appId` of the newly created app registration created prior to deployment | 45782e73-012e-4ef3-9ecb-560157c8e927
tenant | The Azure AD tenant name | blueskyabove.onmicrosoft.com
roleAssignmentGuid | A guid that uniquely identifies the assignment of a user to a role. | b1b9fffc-112e-4d14-b0d5-611b16222c05

## Post-deployment

After deployment, use the Azure CLI to create a secret in the newly created Azure Key Vault. The secret must be named `multiple-apis-client-secret` and must have the password used when creating the app registration in the pre-deployment step above.

````bash
#Find the new KeyVault
az keyvault list --resource-group multiple-apis --query "[].{name:name}"
vaultname=$(az keyvault list --resource-group aad-dotnet-multiple-apis --query "[0].name")
az keyvault secret set --vault-name $vaultname --name 'multiple-apis-client-secret' --value $clientSecret
````

