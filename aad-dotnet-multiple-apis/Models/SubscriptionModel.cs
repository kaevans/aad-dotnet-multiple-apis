using Newtonsoft.Json;
using System.Collections.Generic;

namespace aad_dotnet_multiple_apis.Models
{
    public class SubscriptionModel
    {
        [JsonProperty("value")]
        public List<Subscription> Subscriptions { get; set; }
    }

    public class Subscription
    {
        public string id { get; set; }
        public string subscriptionId { get; set; }
        public string displayName { get; set; }
        public string state { get; set; }

        [JsonProperty("subscriptionPolicies")]
        public SubscriptionPolicies Policies { get; set; }
    }

    public class SubscriptionPolicies
    {
        public string locationPlacementId { get; set; }
        public string quotaId { get; set; }
        public string spendingLimit { get; set; }
    }
}