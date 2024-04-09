namespace MailSender.Models
{
    public class RequestBody
    {
        public string SenderEmail { get; set; }
        public string SenderName { get; set;}
        public string? SenderPW { get; set;} = string.Empty;
        public string RecieverEmailAddress { get; set; }
        public string RecieverName { get; set; }
        public string Subject { get; set; }
        public string EmailBodyContent { get; set; }
    }
}
