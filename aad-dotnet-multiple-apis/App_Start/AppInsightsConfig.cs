using Microsoft.ApplicationInsights.Extensibility;
using System.Configuration;

namespace aad_dotnet_multiple_apis
{
    public class AppInsightsConfig
    {

        public static void RegisterAppInsights()
        {
            TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["InstrumentationKey"];
        }

    }
}