namespace CloudTwilioGateway.Models.SmsModel
{
    public class Sms
    {
        public string Sender { get; set; }
        public string TimeStamp { get; set; }
        public string Text { get; set; }
        public string MsgId { get; set; }
        public string ApiKey { get; set; }
        public string Timezone { get; set; }
        public byte[] BinaryText { get; set; }

    }
}
