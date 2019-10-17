using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudBasedSmsGateway.Models.AppSettings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        public ActionResult ReceiveSms([FromQuery] string From, [FromQuery] string Body)
        {
            var response = MessageResource.Create(
                body: "We received your message and we will give you feedback as soon as it possible",
                from: new Twilio.Types.PhoneNumber(twilioAppConfig.TwilioPhoneNumber),
                to: new Twilio.Types.PhoneNumber(From)
            );
            return Ok();
        }
    }
}