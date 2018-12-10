using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using Microsoft.IdentityModel.Tokens; 
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.ActiveDirectory;
using Owin;

namespace aad_dotnet_webapi_onbehalfof
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                {
                    Tenant = ConfigurationManager.AppSettings["ida:Tenant"],
                    TokenValidationParameters = new TokenValidationParameters {
                        SaveSigninToken = true,
                        ValidAudience = ConfigurationManager.AppSettings["ida:Audience"]
                    }

                }                
                );
        }
    }
}
