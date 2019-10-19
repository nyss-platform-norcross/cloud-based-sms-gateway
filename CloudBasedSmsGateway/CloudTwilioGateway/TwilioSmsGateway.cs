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
        private static HttpClient _httpClient;
        private static string _twilioSid;
        private static string _twilioToken;
        private static string _apiUrl;
        private static string _apiKey;
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
            _twilioSid = config.GetValue<string>("TwilioSid");
            _twilioToken = config.GetValue<string>("TwilioToken");
            _apiUrl = config.GetValue<string>("ApiUrl");
            _apiKey = config.GetValue<string>("ApiKey");
            var streamReader = new StreamReader(req.Body);
            var requestBody = await streamReader.ReadToEndAsync();
            streamReader.Dispose();
            var from = HttpUtility.ParseQueryString(requestBody).Get("From");
            var to = HttpUtility.ParseQueryString(requestBody).Get("To");
            var body = HttpUtility.ParseQueryString(requestBody).Get("Body");
            var messageSid = HttpUtility.ParseQueryString(requestBody).Get("messageSid");
            _log.LogInformation("NEW SMS - FROM:{0} - TO:{1} - MESSAGESID:{2}", from, to, messageSid);
            var sms = CreateSmsFormat(from, body, messageSid);
            if (sms == null)
                return new OkObjectResult(ResponseFactory(1));
            var res = await VerifySms(sms, to);
            if (!res)
                return new OkObjectResult(ResponseFactory(3));
            var responseAPi = await SendSmsToApiAsync(sms);
            return responseAPi.Equals("") ? new OkObjectResult(ResponseFactory(2)) : new OkObjectResult(responseAPi);
        }
        private static Sms CreateSmsFormat(string from, string body, string msgId)
        {
            Sms newSms = null;
            const string patternNumber = @"^[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\s\./0-9]*$";
            if (Regex.IsMatch(from, patternNumber))
            {
                newSms = new Sms
                {
                    Text = body,
                    Sender = from,
                    TimeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                    BinaryText = Encoding.ASCII.GetBytes(body),
                    MsgId = msgId,
                    ApiKey = _apiKey
                };
                _log.LogInformation("SMS FORMATED - MESSAGESID:{0}", newSms.MsgId);
            }
            else
                _log.LogWarning("PROBLEM WITH PHONE NUMBER OF SMS ID:{0}", msgId);
            return newSms;
        }
        private static async Task<string> SendSmsToApiAsync(Sms sms)
        {
            if (_httpClient == null)
                _httpClient = new HttpClient();
            try
            {
                var httpContent = new StringContent(JsonConvert.SerializeObject(sms), Encoding.UTF8, "application/json");
                var res = await _httpClient.PostAsync(_apiUrl + "/api/SmsGateway/", httpContent);
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
        private static async Task<bool> VerifySms(Sms sms, string to)
        {
            MessageResource message;
            try
            {
                TwilioClient.Init(_twilioSid, _twilioToken);
                message = await MessageResource.FetchAsync(sms.MsgId);
            }
            catch (Exception e)
            {
                _log.LogError("ERROR WHILE FETCHING DATA FROM TWILIO - ERROR:{0}", e.Message);
                return false;
            }
            if ( message != null && message.Sid == sms.MsgId && message.Body == sms.Text && message.To == to && message.From.ToString() == sms.Sender)
            {
                _log.LogInformation("SMS FOUND INSIDE TWILIO DATABASE - MESSAGESID:{0}", sms.MsgId);
                return true;
            }
            _log.LogWarning("SMS NOT FOUND INSIDE TWILIO DATABASE - MESSAGESID:{0}", sms.MsgId);
            return false;
        }
        private static string ResponseFactory(int number)
        {
            switch (number)
            {
                case 0:
                    return "We received your message and we will analysis it as soon as it possible";
                case 1:
                    return "Sorry, there is a problem inside your sms. Your phone number seems not supported.";
                case 2:
                    return "Sorry there is a problem during the treatment of your data. Please try again.";
                case 3:
                    return "It seems that your SMS is not from the correct provider.";
                default:
                    return "We received your message and we will analysis it as soon as it possible";
            }
        }
    }
}
