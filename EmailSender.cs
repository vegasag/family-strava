#r "Newtonsoft.Json"

using System;
using System.Net;
using Newtonsoft.Json;
using System.Net.Mail;

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Verbose($"Webhook was triggered!");

    string jsonContent = await req.Content.ReadAsStringAsync();
    dynamic data = JsonConvert.DeserializeObject(jsonContent);
    
    // do some data validation... skipped this for demo purpose only.
    // if validation failes -> HttpStatusCode.BadRequest should be returned as HTTP Status
    
    bool isImportantEmail = bool.Parse(data.isImportant.ToString());
    string fromEmail = data.fromEmail;
    string toEmail = data.toEmail;
    int smtpPort = 587;
    bool smtpEnableSsl = true;
    string smtpHost = ""; // your smtp host
    string smtpUser = ""; // your smtp user
    string smtpPass = ""; // your smtp password
    string subject = data.subject;
    string message = data.message;
    
    MailMessage mail = new MailMessage(fromEmail, toEmail);
    SmtpClient client = new SmtpClient();
    client.Port = smtpPort;
    client.EnableSsl = smtpEnableSsl;
    client.DeliveryMethod = SmtpDeliveryMethod.Network;
    client.UseDefaultCredentials = false;
    client.Host = smtpHost;
    client.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass);
    mail.Subject = message;
    
    if (isImportantEmail) {
      mail.Priority = MailPriority.High;
    }
    
    mail.Body = message;
    try {
      client.Send(mail);
      log.Verbose("Email sent.");
      return req.CreateResponse(HttpStatusCode.OK, new {
            status = true,
            message = string.Empty
        });
    }
    catch (Exception ex) {
      log.Verbose(ex.ToString());
      return req.CreateResponse(HttpStatusCode.InternalServerError, new {
            status = false,
            message = "Message has not been sent. Check Azure Function Logs for more information."
        });
    }
}