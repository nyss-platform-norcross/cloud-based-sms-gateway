using CloudBasedSmsGateway.Models.AppSettings;
using CloudBasedSmsGateway.Models.SmsModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace CloudBasedSmsGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiveSmsController : ControllerBase
    {
        private readonly TwilioAppConfig twilioAppConfig;
        private readonly HttpClient httpClient;
        public ReceiveSmsController(IOptions<TwilioAppConfig> twilioOptions)
        {
            twilioAppConfig = twilioOptions.Value;
            TwilioClient.Init(twilioAppConfig.TwilioSid, twilioAppConfig.TwilioToken);
            httpClient = new HttpClient();
        }
        [HttpGet]
        public async Task<ActionResult> ReceiveSms([FromQuery] string From, [FromQuery] string Body, [FromQuery] string MessageSid)
        {
            var sms = new Sms
            {
                Sender = From,
                Text = Body,
                BinaryText = Encoding.ASCII.GetBytes(Body),
                MsgId = MessageSid,
                TimeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                ApiKey = Guid.NewGuid().ToString()
            };
            var httpContent = new StringContent(JsonSerializer.Serialize(sms), Encoding.UTF8, "application/json");
            try
            {
                var res = await httpClient.PostAsync("http://0c5d9758.ngrok.io/api/SmsGateway/", httpContent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            var response = MessageResource.Create(
                body: "We received your message and we will analyse it as soon as it possible",
                from: new Twilio.Types.PhoneNumber(twilioAppConfig.TwilioPhoneNumber),
                to: new Twilio.Types.PhoneNumber(From)
            );
            return Ok();
        }
    }
}