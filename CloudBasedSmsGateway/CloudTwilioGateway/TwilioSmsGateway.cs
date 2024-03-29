using CloudTwilioGateway.Models.Api;
using CloudTwilioGateway.Models.SmsModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Twilio;
using Twilio.Base;
using Twilio.Rest.Api.V2010.Account;

namespace CloudTwilioGateway
{
    public static class TwilioSmsGateway
    {
        private static ILogger _log;
        private static HttpClient httpClient;
        private static string twilioSid;
        private static string twilioToken;
        private static string apiUrl;
        [FunctionName("Twilio")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            _log = log;
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            twilioSid = config.GetValue<string>("TwilioSid");
            twilioToken = config.GetValue<string>("TwilioToken");
            apiUrl = config.GetValue<string>("ApiUrl");
            StreamReader streamReader = new StreamReader(req.Body);
            string requestBody = await streamReader.ReadToEndAsync();
            streamReader.Dispose();
            string from = HttpUtility.ParseQueryString(requestBody).Get("From");
            string to = HttpUtility.ParseQueryString(requestBody).Get("To");
            string body = HttpUtility.ParseQueryString(requestBody).Get("Body");
            string messageSid = HttpUtility.ParseQueryString(requestBody).Get("messageSid");
            _log.LogInformation("NEW SMS - FROM:{0} - TO:{1} - MESSAGESID:{2}", from, to, messageSid);
            var sms = CreateSmsFormat(from, body, messageSid);
            if (sms == null)
                return new OkObjectResult(ResponseFactory(1));
            var res = VerifySms(sms, to);
            if (!res)
                return new OkObjectResult(ResponseFactory(3));
            var responseAPi = await SendSmsToApiAsync(sms);
            if (responseAPi.Equals(""))
                return new OkObjectResult(ResponseFactory(2));
            return new OkObjectResult(responseAPi);
        }
        private static Sms CreateSmsFormat(string from, string body, string msgId)
        {
            Sms newSms = null;
            var patternNumber = @"^[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\s\./0-9]*$";
            if (Regex.IsMatch(from, patternNumber))
            {
                newSms = new Sms
                {
                    Text = body,
                    Sender = from,
                    TimeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                    BinaryText = Encoding.ASCII.GetBytes(body),
                    MsgId = msgId,
                    ApiKey = Guid.NewGuid().ToString()
                };
                _log.LogInformation("SMS FORMATED - MESSAGESID:{0}", newSms.MsgId);
            }
            else
                _log.LogWarning("PROBLEM WITH PHONE NUMBER OF SMS ID:{0}", msgId);
            return newSms;
        }
        private static async Task<string> SendSmsToApiAsync(Sms sms)
        {
            if (httpClient == null)
                httpClient = new HttpClient();
            try
            {
                var httpContent = new StringContent(JsonConvert.SerializeObject(sms), Encoding.UTF8, "application/json");
                var res = await httpClient.PostAsync( apiUrl + "/api/SmsGateway/", httpContent);
                if (res.IsSuccessStatusCode)
                {
                    _log.LogInformation("SENDED SMS SUCCESFULL TO THE API - MESSAGESID:{0}", sms.MsgId);
                    var resultString = await res.Content.ReadAsStringAsync();
                    var answer = JsonConvert.DeserializeObject<Answer>(resultString);
                    return answer.FeedbackMessage;
                }
                _log.LogError("SENDED SMS NOT SUCCESFULL TO THE API - MESSAGESID:{0}", sms.MsgId);
                return "";
            }
            catch (HttpRequestException e)
            {
                _log.LogError("ERROR WHILE SENDING SMS TO THE API - MESSAGESID:{0} - ERROR:{1}", sms.MsgId, e.Message);
                return "";
            }
        }
        private static bool VerifySms(Sms sms, string to)
        {
            ResourceSet<MessageResource> messages;
            try
            {
                TwilioClient.Init(twilioSid, twilioToken);
                messages = MessageResource.Read(
                    from: new Twilio.Types.PhoneNumber(sms.Sender),
                    to: new Twilio.Types.PhoneNumber(to)
                );

            }
            catch (Exception e)
            {
                _log.LogError("ERROR WHILE FETCHING DATA FROM TWILIO - ERROR:{0}", e.Message);
                return false;
            }
            foreach (var record in messages)
            {
                if (record.Sid == sms.MsgId && record.Body == sms.Text)
                {
                    _log.LogInformation("SMS FOUND INSIDE TWILIO DATABASE - MESSAGESID:{0}", sms.MsgId);
                    return true;
                }
            }
            _log.LogWarning("SMS NOT FOUND INSIDE TWILIO DATABASE - MESSAGESID:{0}", sms.MsgId);
            return false;
        }
        private static string ResponseFactory(int number)
        {
            switch (number)
            {
                case 0:
                    return "We received your message and we will analyse it as soon as it possible";
                case 1:
                    return "Sorry, there is a problem inside your sms. Please verify the syntax of it";
                case 2:
                    return "Sorry there is a probem during the traitment of your data. Please try again.";
                case 3:
                    return "It seems that your SMS is not from the correct provider.";
                default:
                    return "We received your message and we will analyse it as soon as it possible";
            }
        }
    }
}
