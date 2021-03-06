﻿using DataAccess.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MimeKit;
using MimeKit.Text;
using Services.Classes;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    public class EmailWorkerService : BackgroundService

    {
        private readonly EmailService emailService;
        private readonly IConfiguration configuration;
        private readonly NicheShackContext context;
        private readonly IServiceScope scope;

        public EmailWorkerService(IServiceScopeFactory serviceScopeFactory, EmailService emailService, IConfiguration configuration)
        {
            scope = serviceScopeFactory.CreateScope();
            context = scope.ServiceProvider.GetRequiredService<NicheShackContext>();
            this.emailService = emailService;
            this.configuration = configuration;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000 * 5);




                while (emailService.emailSetupMethods.Count > 0)
                {
                    EmailSetupMethod emailSetupMethod = emailService.emailSetupMethods[0];

                    var func = emailSetupMethod.Func;

                    await func(context, emailSetupMethod.Args);

                    emailService.emailSetupMethods.Remove(emailSetupMethod);
                }



                while (emailService.emails.Count > 0)
                {
                    EmailMessage emailMessage = emailService.emails[0];

                    string emailContent = await GetEmailContent(emailMessage.EmailType);

                    string emailBody = await GetEmailBody(emailContent, emailMessage.EmailProperties);

                    MimeMessage email = GetEmail(emailMessage.EmailAddress, emailMessage.Subject, emailBody);

                    await SendEmail(email);


                    emailService.emails.Remove(emailMessage);


                }



            }
        }


        private async Task<string> GetEmailContent(EmailType emailType)
        {
            string emailName = Regex.Replace(emailType.ToString(), "[A-Z]", " $0").Trim();
            return await context.Emails.Where(x => x.Name == emailName).Select(x => x.Content).SingleOrDefaultAsync();
        }


        private async Task<string> GetEmailBody(string content, EmailProperties emailProperties)
        {
            // Deserialize the content into an EmailPage object
            EmailPage emailPage = JsonSerializer.Deserialize<EmailPage>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });


            // Create the body
            string body = await emailPage.CreateBody(context);
            return emailProperties.Set(body);
        }



        private MimeMessage GetEmail(string recipient, string subject, string body)
        {
            MimeMessage email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(configuration["Email:Sender"]);
            email.To.Add(MailboxAddress.Parse(recipient));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = body };

            return email;
        }


        private async Task SendEmail(MimeMessage email)
        {
            SmtpClient smtp = new SmtpClient();
            await smtp.ConnectAsync(configuration["Email:Host"], Convert.ToInt32(configuration["Email:Port"]), (SecureSocketOptions)Convert.ToInt32(configuration["Email:SecureSocketOption"]));
            await smtp.AuthenticateAsync(configuration["Email:UserName"], configuration["Email:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }


        public override Task StopAsync(CancellationToken cancellationToken)
        {
            scope.Dispose();

            return base.StopAsync(cancellationToken);
        }
    }
}
