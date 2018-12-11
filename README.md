# aad-dotnet-multiple-apis

Example of using multiple APIs with ADAL to Azure-protected resources.

## Overview of solution

The solution is an ASP.NET MVC web app that authenticates the user using OpenId Connect with the ADAL library and Azure AD. The app is deployed to an Azure App Service that has been assigned a Managed Service Identity (MSI).

The MSI is given permission to an Azure Key Vault where the AAD app's clientSecret is stored, avoiding storing any secrets in configuration. 

Once the user logs in, you can choose from various APIs in the menu to test accessing various services such as Azure Management API, Azure Storage, Azure SQL Database, Azure AD Graph API, and Microsoft Graph API.

The cached access tokens for the application are stored in an Azure SQL Database. Click the Purge button on the home page to delete all of the cached tokens to observe the behavior in the code.

The application requires the user to consent to the requested application permissions. If you want to make changes to the requested permissions, you will need to reconsent after the changes are made. Click the Reconsent button on the home screen to reconsent to the new permissions.

## Deploying the template

To deploy the solution:

- Edit the following values at the top of the `azuredeploy.sh` file:

Variable | Description
-------- | -----------
rg | Name of resource group to create where all resources are deployed
location | Azure region where resources are deployed
tenantName | Name of your AAD tenant

- Go to the Azure Portal and **open** the Azure Cloud Shell using Bash.
- Use the upload button to **upload** `azuredeploy.json` and `azuredeploy.sh` from the paas-deploy project.
- Deploy the solution using `bash azuredeploy.sh`

## Running the solution locally

If you want to run the solution locally, first deploy the solution as described above. Then open the Application Settings blade for each web application that was deployed and update the corresponding `web.config` file locally. Update the DemoDb connection string for the web project to point to the newly deployed Azure SQL Database, and make sure to enable your client IP in the Azure SQL Database's server firewall rules.

Both the MVC web application and the Web API use Managed Service Identity to communicate with Azure Key Vault. To debug locally, open the solution in Visual Studio 2017 and go to the **Options / Azure Service Authentication / Account Selection** screen. The account that you run as must also have permission to Get secrets from the created Azure Key Vault, which is added for you as part of the ARM template deployment.

![Azure Service Authentication Account Selection screen in Visual Studio 2017](images/azure-service-authentication.png)

## Troubleshooting

- If you get an error that the `azuredeploy.sh` file has an invalid r character, the line endings have been changed from LF to CRLF. Open the file using Visual Studio Code, and at the bottom right of the screen click on the CRLF to change the end of line sequence to LF.
- If debugging locally and you try to access the Azure SQL Database page, it will likely fail. Make sure to update the Firewall Rules for the logical server to add your client IP. To ensure this works, open the Azure SQL Database using SQL Server Management Studio and you will be prompted to update the firewall rules.