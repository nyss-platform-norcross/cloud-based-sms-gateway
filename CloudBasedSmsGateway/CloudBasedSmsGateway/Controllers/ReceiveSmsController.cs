using CloudBasedSmsGateway.Models.AppSettings;
using CloudBasedSmsGateway.Models.SmsModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace CloudBasedSmsGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiveSmsController : ControllerBase
    {
        private readonly TwilioAppConfig twilioAppConfig;
        public ReceiveSmsController(IOptions<TwilioAppConfig> twilioOptions)
        {
            twilioAppConfig = twilioOptions.Value;
            TwilioClient.Init(twilioAppConfig.TwilioSid, twilioAppConfig.TwilioToken);
        }
        [HttpGet]
        public ActionResult ReceiveSms([FromQuery] string From, [FromQuery] string Body, [FromQuery] string MessageSid)
        {
            var sms = new Sms
            {
                Sender = From,
                Text = Body,
                BinaryText = Encoding.ASCII.GetBytes(Body),
                MsgId = MessageSid,
                TimeStamp = DateTime.UtcNow.ToString("YYYYMMddHHmmss")
            };
            var response = MessageResource.Create(
                body: "We received your message and we will give you feedback as soon as it possible",
                from: new Twilio.Types.PhoneNumber(twilioAppConfig.TwilioPhoneNumber),
                to: new Twilio.Types.PhoneNumber(From)
            );
            return Ok();
        }
    }
}