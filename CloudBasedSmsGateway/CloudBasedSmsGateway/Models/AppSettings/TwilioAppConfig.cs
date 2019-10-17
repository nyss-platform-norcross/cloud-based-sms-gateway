using Newtonsoft.Json;

namespace CloudBasedSmsGateway.Models.AppSettings
{
    public class TwilioAppConfig
    {
        [JsonProperty("TwilioSid")]
        public string TwilioSid { get; set; }

        [JsonProperty("TwilioToken")]
        public string TwilioToken { get; set; }

        [JsonProperty("TwilioPhoneNumber")]
        public string TwilioPhoneNumber { get; set; }
    }
}
