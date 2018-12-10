using Microsoft.ApplicationInsights.Extensibility;
using System.Configuration;

namespace aad_dotnet_webapi_onbehalfof
{
    public class AppInsightsConfig
    {
        public static void RegisterAppInsights()
        {
            TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["InstrumentationKey"];
        }
    }
}