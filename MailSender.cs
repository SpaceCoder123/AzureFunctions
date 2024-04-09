using MailKit.Net.Smtp;
using MailKit.Security;
using MailSender.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MailSender
{
    public static class MailSender
    {
        [FunctionName("MailSender")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            try
            {
                log.LogInformation("MailSender service has been triggered.");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                RequestBody data = JsonConvert.DeserializeObject<RequestBody>(requestBody);

                string payloadJson = JsonConvert.SerializeObject(data);
                log.LogInformation($"This is the payload: {payloadJson}");

                var config = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                string smtpHost = config["Credentials:SmtpHost"];
                int smtpPort = int.Parse(config["Credentials:SmtpPort"]);
                string smtpUsername = config["Credentials:SmtpUsername"];
                string smtpPassword = config["Credentials:SmtpPassword"];

                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(data.SenderEmail));
                email.To.Add(MailboxAddress.Parse(data.RecieverEmailAddress));
                email.Subject = data.Subject;
                email.Body = new TextPart(TextFormat.Text) { Text = data.EmailBodyContent };

                log.LogInformation($"Send email from {data.SenderName} to {data.RecieverName}");

                using (var smtpClient = new SmtpClient())
                {
                    await smtpClient.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                    await smtpClient.AuthenticateAsync(smtpUsername, smtpPassword);

                    await smtpClient.SendAsync(email);

                    await smtpClient.DisconnectAsync(true);
                }

                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while sending the email.");
                return new BadRequestObjectResult("Failed to send the email.");
            }
        }
    }
}
