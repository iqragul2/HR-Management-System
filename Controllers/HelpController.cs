
using HRGroup.Models;
using Microsoft.AspNetCore.Mvc;


namespace HRGroup.Controllers
{
    public class HelpController : Controller
    {

        private readonly ILogger<HelpController> _logger;

        private readonly EmailService emailService;

        public HelpController(ILogger<HelpController> logger, EmailService emailService)
        {
            _logger = logger;
            this.emailService = emailService;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Index(ContactU emailMessage)
        {
            await emailService.SendEmailAsync(emailMessage);
            ViewBag.SuccessMessage = "Email sent successfully!";

            // Clear the form fields
            ModelState.Clear(); // Clears validation and binds empty model
            return View(new ContactU()); // Return empty model to clear form
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            //return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public IActionResult FAQs()
        {
            return View();
        }

    }
}
