
using Microsoft.AspNetCore.Mvc;
using RE_Waste.Models;
using System.Diagnostics;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace RE_Waste.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(string name, string email, string message)
        {

            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(message))
            {
                ViewBag.Error = "Please fill in all fields.";
                return View();
            }

            if (name.Length > 100 ||
                email.Length > 254 ||
                message.Length > 2000)
            {
                ViewBag.Error = "One or more fields are too long.";
                return View();
            }

            if (!System.Net.Mail.MailAddress.TryCreate(email, out var address) ||
                address.Address != email)
            {
                ViewBag.Error = "Please enter a valid email address.";
                return View();
            }


            try
            {
                var senderEmail = HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()["EmailSettings:SenderEmail"];

                var appPassword = HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()["EmailSettings:AppPassword"];

                var mail = new MimeMessage();
                mail.From.Add(MailboxAddress.Parse(senderEmail));
                mail.To.Add(MailboxAddress.Parse(senderEmail));
                mail.ReplyTo.Add(new MailboxAddress(name, email));
                mail.Subject = "New Re-Waste Contact Message";

                mail.Body = new TextPart("plain")
                {
                    Text = $"Name: {name}\nEmail: {email}\n\nMessage:\n{message}"
                };

                using var client = new SmtpClient();

                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, appPassword);
                await client.SendAsync(mail);
                await client.DisconnectAsync(true);

                TempData["Success"] = "Your message has been sent successfully.";
                return RedirectToAction("Contact");
            }
            catch (Exception)
            {
                ViewBag.Error = "Sorry, your message could not be sent. Please try again.";
                return View();
            }
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Displays a friendly page when an unexpected error occurs.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        // Checks the HTTP status code and displays the correct error page.
        public IActionResult StatusCode(int code)
        {
            if (code == 404)
            {
                return View("NotFound");
            }

            return View("Error");
        }
    }
}
