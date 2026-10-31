using System.IO.Compression;
using System.Net;
using System.Net.Mail;
using kontrolnaya.Framework.Attributes;

namespace kontrolnaya.Controllers
{
    [HttpController("home-work")]
    internal class HomeworkController
    {
        [Get("send")]
        public string Send(string to)
        {
            if (string.IsNullOrWhiteSpace(to) || !to.Contains("@"))
                return "Ошибка: неверный email получателя (to)";

            try
            {
                string zipPath = Path.Combine(Path.GetTempPath(), "project.zip");
                string sourceDir = Path.Combine(Directory.GetCurrentDirectory(), "static"); 

                if (File.Exists(zipPath)) File.Delete(zipPath);
                ZipFile.CreateFromDirectory(sourceDir, zipPath);
                Console.WriteLine($"[ZIP] Архив создан: {zipPath}");
                
                
                string smtpHost = "smtp.yandex.ru";
                int smtpPort = 587; 
                string smtpUser = "emilemilemil2845@yandex.ru"; 
                string smtpPass = "rjyixqxgeitogxmb"; 
                
                MailAddress from = new MailAddress(smtpUser, "Сообщение");
                MailAddress toAddress = new MailAddress(to);
                
                MailMessage m = new MailMessage(from, toAddress);
                
                m.Subject = "Домашнее задание";
                
                m.Body = $@"
                    <p><b>ФИО:</b>Ибрагимов Эмиль Ринатович</p>
                    <p><b>Группа:</b>11-541</p>
                ";
                
                m.IsBodyHtml = true;
                
                
                
                m.Attachments.Add(new Attachment("C:/Users/ibrem/Desktop/satisfactory_server.zip"));
                
                
                
                SmtpClient smtp = new SmtpClient(smtpHost, smtpPort);
                
                smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                smtp.EnableSsl = true;
                
                smtp.Send(m);
                
                Console.WriteLine($"[EMAIL] Письмо успешно отправлено на {to}");

                return $"OK: Сообщение с архивом отправлено на {to}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL ERROR] {ex.Message}");
                return $"Ошибка отправки: {ex.Message}";
            }
        }
    }
}