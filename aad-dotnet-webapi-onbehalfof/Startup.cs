using Microsoft.Owin;
using Owin;

[assembly:OwinStartup(typeof(aad_dotnet_webapi_onbehalfof.Startup))]
namespace aad_dotnet_webapi_onbehalfof
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
