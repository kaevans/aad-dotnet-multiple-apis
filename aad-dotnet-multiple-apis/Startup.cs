using Microsoft.Owin;
using Owin;


[assembly: OwinStartup(typeof(aad_dotnet_multiple_apis.Startup))]

namespace aad_dotnet_multiple_apis
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
