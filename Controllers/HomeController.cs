using Microsoft.AspNetCore.Mvc;
using HRGroup.Data;
using HRGroup.Models;
using Microsoft.EntityFrameworkCore;


namespace HRGroup.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RecruitmentDbContext db;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor contx;
        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env, RecruitmentDbContext db, IHttpContextAccessor contx)
        {
            this.contx = contx;
            this.db = db;
            _logger = logger;
            _env = env;

        }
        private string GenerateApplicantId()
        {
            var lastApplicant = db.Applicants
                .OrderByDescending(a => a.ApplicantId)
                .FirstOrDefault();

            if (lastApplicant == null)
                return "A0001";

            var lastId = lastApplicant.ApplicantId;

            if (lastId.Length < 5 || !int.TryParse(lastId.Substring(1), out int lastNum))
            {
                // fallback if format is unexpected
                lastNum = 0;
            }

            return $"A{(lastNum + 1).ToString("D4")}";
        }

        [HttpGet]
        public IActionResult UserPage()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0)
                return RedirectToAction("Login", "Home");

            var employee = db.Employees.FirstOrDefault(e => e.UserId == userId);
            if (employee == null || !employee.ProfileCompleted.GetValueOrDefault())
                return RedirectToAction("CompleteProfileFirst", "Home");

            var applicant = db.Applicants.FirstOrDefault(a => a.Email == employee.Email);

            if (applicant != null)
            {
                // Check ApplicantVacancy status to update Applicant status
                var selectedVacancy = db.ApplicantVacancies
                    .FirstOrDefault(av => av.ApplicantId == applicant.ApplicantId && av.Status == "Selected");

                if (selectedVacancy != null)
                {
                    applicant.Status = "Hired";
                }
                else
                {
                    var rejectedVacancy = db.ApplicantVacancies
                        .FirstOrDefault(av => av.ApplicantId == applicant.ApplicantId && av.Status == "Rejected");

                    if (rejectedVacancy != null)
                    {
                        applicant.Status = "Banned";
                    }
                }

                // Save applicant status update
                db.SaveChanges();
            }

            var appliedVacancies = applicant != null ? db.ApplicantVacancies
                .Include(av => av.Vacancy)
                    .ThenInclude(v => v.Department)
                .Where(av => av.ApplicantId == applicant.ApplicantId)
                .ToList() : new List<ApplicantVacancy>();

            var interviews = applicant != null ? db.Interviews
                .Include(i => i.ApplicantVacancy)
                    .ThenInclude(av => av.Vacancy)
                        .ThenInclude(v => v.Department)
                .Include(i => i.Interviewer)
                .Where(i => i.ApplicantVacancy.ApplicantId == applicant.ApplicantId)
                .ToList() : new List<Interview>();

            var openVacancies = db.Vacancies.Include(v => v.Department)
                .Where(v => v.Status == "Open")
                .ToList();

            ViewBag.Employee = employee;
            ViewBag.AppliedVacancies = appliedVacancies;
            ViewBag.Interviews = interviews;
            ViewBag.AllVacancies = openVacancies;

            return View();
        }


        [HttpPost]
        [ServiceFilter(typeof(RequireProfileAttribute))]
        public IActionResult ApplyToVacancy(string VacancyId, string ApplicantName, string ApplicantEmail, string ApplicantPhone)
        {
            if (string.IsNullOrEmpty(VacancyId))
            {
                TempData["Error"] = "Invalid vacancy selected.";
                return RedirectToAction("Dashboard");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId == 0)
            {
                TempData["Error"] = "Please login first.";
                return RedirectToAction("Login", "Account");
            }

            // Applicant record find ya create karo
            var applicant = db.Applicants.FirstOrDefault(a => a.Email == ApplicantEmail);

            if (applicant == null)
            {
                applicant = new Applicant
                {
                    ApplicantId = GenerateApplicantId(),  
                    Name = ApplicantName,
                    Email = ApplicantEmail,
                    PhoneNumber = ApplicantPhone,
                    AppliedDate = DateTime.Now,
                    Status = "Not in Process"
                };
                db.Applicants.Add(applicant);
                db.SaveChanges();
            }
            else
            {
                applicant.Name = ApplicantName;
                applicant.PhoneNumber = ApplicantPhone;
                db.SaveChanges();
            }

            // Check if already applied
            bool alreadyApplied = db.ApplicantVacancies
                .Any(av => av.ApplicantId == applicant.ApplicantId && av.VacancyId == VacancyId);

            if (alreadyApplied)
            {
                TempData["NotificationMessage"] = "You have already applied for this vacancy.";
                TempData["NotificationType"] = "warning";
                return RedirectToAction("Dashboard");
            }

            // Create new ApplicantVacancy record
            var application = new ApplicantVacancy
            {
                ApplicantId = applicant.ApplicantId,
                VacancyId = VacancyId,
                Status = "In Process",
                AttachedDate = DateTime.Now
            };

            db.ApplicantVacancies.Add(application);
            db.SaveChanges();

            TempData["NotificationMessage"] = "Your application has been successfully submitted!";
            TempData["NotificationType"] = "success";

            return RedirectToAction("UserPage");
        }



        [HttpGet]
        public IActionResult VacancyDetails(string id)
        {
            var vacancy = db.Vacancies.FirstOrDefault(v => v.VacancyId == id);
            if (vacancy == null)
                return NotFound();


            return View(vacancy);
        }
        [HttpGet]
        public IActionResult Vacancies(string departmentId)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }
            var departments = db.Departments.AsNoTracking().ToList();

            List<Vacancy> vacancies;

            if (!string.IsNullOrEmpty(departmentId))
            {
                vacancies = db.Vacancies
                    .AsNoTracking()
                    .Where(v => v.DepartmentId == departmentId && v.Status == "Open")
                    .ToList();
            }
            else
            {
                vacancies = db.Vacancies
                    .AsNoTracking()
                    .Where(v => v.Status == "Open")
                    .ToList();
            }

            ViewBag.SelectedDepartmentId = departmentId ?? "";

            return View(Tuple.Create(departments, vacancies));
        }
        public IActionResult Dashboard()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }

            var user = db.Users.FirstOrDefault(u => u.UserId == userId);

            if (user != null && user.RoleId == 2)
            {
                // Find the corresponding employee
                var employee = db.Employees.FirstOrDefault(e => e.UserId == userId);

                if (employee != null)
                {
                    // Save user info (first name, last name) and employee id / full name
                    HttpContext.Session.SetString("FirstName", user.FirstName ?? "");
                    HttpContext.Session.SetString("LastName", user.LastName ?? "");
                    HttpContext.Session.SetString("FullName", employee.FullName ?? "");
                    HttpContext.Session.SetInt32("EmployeeId", employee.EmployeeId);

                    // Determine if it's a new user
                    bool isNewUser = HttpContext.Session.GetInt32("IsNewUser") == 1;
                    ViewBag.IsNewUser = isNewUser;
                    ViewBag.HasProfile = true;
                }
                else
                {
                    ViewBag.HasProfile = false;
                }
            }
            else
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(User model)
        {
            // Default role is User (assume RoleId = 2 for User)
            model.RoleId = 2; // user

            db.Users.Add(model);
            db.SaveChanges();

            return RedirectToAction("Login");

        }

        [HttpGet]
        public IActionResult Login()
        {


            return View();
        }



        [HttpPost]
        public IActionResult Login(User data, string email, string password)
        {
            if (ModelState.IsValid == false)
            {
                var uservalidity = db.Users.Where(user => user.Email == data.Email && user.Password == data.Password).ToList();

                if (uservalidity.Count > 0)
                {
                    // ✅ Store email in TempData
                    TempData["Email"] = data.Email;
                    var employee = db.Employees.FirstOrDefault(e => e.UserId == uservalidity[0].UserId);

                    if (employee != null)
                    {
                        HttpContext.Session.SetInt32("EmployeeId", employee.EmployeeId);
                    }
                    if (uservalidity[0].RoleId == 2) // USER
                    {
                        HttpContext.Session.SetInt32("UserId", uservalidity[0].UserId);
                        HttpContext.Session.SetString("Userfirstname", uservalidity[0].FirstName);
                        HttpContext.Session.SetString("Userlastname", uservalidity[0].LastName);
                        HttpContext.Session.SetString("Userrole", Convert.ToString(uservalidity[0].RoleId));
                        HttpContext.Session.SetString("Email", uservalidity[0].Email); // Already done

                        return RedirectToAction("Dashboard", "Home");
                    }
                    else if (uservalidity[0].RoleId == 3) // HR
                    {
                        var recruiter = db.RecruiterRequests.FirstOrDefault(r => r.UserId == uservalidity[0].UserId);

                        if (recruiter != null && recruiter.Status == "Approved")
                        {
                            HttpContext.Session.SetInt32("UserId", uservalidity[0].UserId);
                            HttpContext.Session.SetString("Userfirstname", uservalidity[0].FirstName);
                            HttpContext.Session.SetString("Userlastname", uservalidity[0].LastName);
                            HttpContext.Session.SetString("Userrole", Convert.ToString(uservalidity[0].RoleId));

                            return RedirectToAction("Dashboard", "HR");
                        }
                        else
                        {
                            // If recruiter status is Rejected or Pending, show error and stay on login page
                            ViewBag.Error = "Your recruiter account is not approved. Please contact admin.";
                            return View();
                        }
                    }
                    else if (uservalidity[0].RoleId == 1) // ADMIN
                    {
                        HttpContext.Session.SetInt32("UserId", uservalidity[0].UserId);
                        HttpContext.Session.SetString("Userfirstname", uservalidity[0].FirstName);
                        HttpContext.Session.SetString("Userlastname", uservalidity[0].LastName);
                        HttpContext.Session.SetString("Userrole", Convert.ToString(uservalidity[0].RoleId));

                        return RedirectToAction("Dashboard", "Admin");
                    }
                }
            }

            ViewBag.Error = "Invalid login credentials.";
            return View();
        }


        [HttpGet]
        public IActionResult ChangePassword()
        {
            var email = HttpContext.Session.GetString("Email");
            if (email == null)
            {
                return RedirectToAction("Login");
            }

            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                ViewBag.CurrentPassword = user.Password; // ❌ Only for demo/test
            }

            return View();
        }


        [HttpPost]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var email = HttpContext.Session.GetString("Email");

            if (email == null)
            {
                return RedirectToAction("Login");
            }

            var user = db.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                ViewBag.Error = "User not found.";
                return View();
            }


            if (newPassword != confirmPassword)
            {
                ViewBag.CurrentPassword = user.Password; // refill readonly field
                ViewBag.Error = "New passwords do not match.";
                return View();
            }

            // Update password
            user.Password = newPassword;
            user.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            ViewBag.Message = "Password updated successfully.";
            ViewBag.CurrentPassword = user.Password;
            return View();
        }



        [HttpGet]
        public IActionResult ResetPassword()
        {
            string email = HttpContext.Session.GetString("Email");
            ViewBag.Email = email; // Pass to the view
            return View();
        }


        [HttpPost]
        public IActionResult ResetPassword(IFormCollection form)
        {
            try
            {
                string email = Request.Form["Email"];
                string newPassword = Request.Form["NewPassword"];
                string confirmPassword = Request.Form["ConfirmPassword"];

                // Check if all fields are filled
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
                {
                    ViewBag.Error = "Please fill all the fields.";
                    ViewBag.Email = email;  // Keep the email in the form
                    return View();
                }

                // Check if the new password and confirmation password match
                if (newPassword != confirmPassword)
                {
                    ViewBag.Error = "Passwords do not match.";
                    ViewBag.Email = email;  // Keep the email in the form
                    return View();
                }

                // Check if the email exists in the database
                var user = db.Users.FirstOrDefault(u => u.Email == email);
                if (user == null)
                {
                    ViewBag.Error = "User not found with this email.";
                    ViewBag.Email = email;  // Keep the email in the form
                    return View();
                }

                // Password reset logic (ensure you hash the password in production)
                user.Password = newPassword;
                user.UpdatedAt = DateTime.Now;

                // Save the changes to the database
                db.SaveChanges();

                // Redirect to Login page after successful reset
                TempData["Message"] = "Password has been reset successfully.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // Log the exception and show a generic error
                _logger.LogError(ex, "Error occurred while resetting password.");
                ViewBag.Error = "An unexpected error occurred. Please try again.";
                return View();
            }
        }
        public IActionResult AboutUs()
        {

            var teamMembers = db.AboutUs.ToList();// no async
            return View(teamMembers);

        }
        public IActionResult Test()
        {
            return View();
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            return RedirectToAction("Login", "Home");
        }



        public IActionResult Help()
        {
            return View();
        }

        public IActionResult Show()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var employee = db.Employees.FirstOrDefault(e => e.UserId == userId);

            if (employee == null)
            {
                return Content("No profile found.");
            }

            return View(employee);
        }

        [HttpGet]
        public IActionResult Create()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var user = db.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null || user.RoleId != 2)
            {
                return Content("Unauthorized to create employee.");
            }

            var employee = new Employee
            {
                Designation = "User",
                DepartmentId = null
            };
            return View();
        }

        [HttpPost]
        public IActionResult Create(Employee employee, IFormFile ProfileImage)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var user = db.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null || user.RoleId != 2)
            {
                return Content("Unauthorized to create employee.");
            }

            // Default values
            employee.Designation = "User";
            employee.DepartmentId = null;

            // Find applicant by email
            var applicant = db.Applicants.FirstOrDefault(a => a.Email == user.Email);
            if (applicant != null)
            {
                // Check if applicant is hired (status = Selected)
                var hiredVacancy = db.ApplicantVacancies
                    .FirstOrDefault(av => av.ApplicantId == applicant.ApplicantId && av.Status == "Selected");

                if (hiredVacancy != null)
                {
                    var vacancy = db.Vacancies.FirstOrDefault(v => v.VacancyId == hiredVacancy.VacancyId);
                    if (vacancy != null)
                    {
                        // Set employee department and designation from vacancy
                        employee.DepartmentId = vacancy.DepartmentId;
                        employee.Designation = vacancy.Title;
                    }
                }
            }

            // Save profile image if uploaded
            if (ProfileImage != null)
            {
                var filePath = Path.Combine("wwwroot/images", ProfileImage.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ProfileImage.CopyTo(stream);
                }
                employee.ProfileImageUrl = "/images/" + ProfileImage.FileName;
            }
            employee.ProfileCompleted = true;
            employee.UserId = userId;
            db.Employees.Add(employee);
            db.SaveChanges();

            return RedirectToAction("Show");
        }

        [HttpGet]
        public IActionResult Edit(int employeeId)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var user = db.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null || user.RoleId != 2)
            {
                return Content("Unauthorized to edit profile.");
            }

            var employee = db.Employees.FirstOrDefault(e => e.EmployeeId == employeeId && e.UserId == userId);
            if (employee == null)
            {
                return Content("Profile not found.");
            }

            return View(employee);
        }

        [HttpPost]
        public IActionResult Edit(Employee updatedEmployee, IFormFile ProfileImage)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var user = db.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null || user.RoleId != 2)
            {
                return Content("Unauthorized to edit profile.");
            }

            var existingEmployee = db.Employees.FirstOrDefault(e => e.EmployeeId == updatedEmployee.EmployeeId && e.UserId == userId);
            if (existingEmployee == null)
            {
                return Content("Profile not found.");
            }

            existingEmployee.FullName = updatedEmployee.FullName;
            existingEmployee.Email = updatedEmployee.Email;
            existingEmployee.PhoneNumber = updatedEmployee.PhoneNumber;
            existingEmployee.Address = updatedEmployee.Address;
            existingEmployee.Gender = updatedEmployee.Gender;
            existingEmployee.DateOfBirth = updatedEmployee.DateOfBirth;

            // Save new profile image if uploaded
            if (ProfileImage != null)
            {
                var filePath = Path.Combine("wwwroot/images", ProfileImage.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ProfileImage.CopyTo(stream);
                }
                existingEmployee.ProfileImageUrl = "/images/" + ProfileImage.FileName;
            }

           db.SaveChanges();

            return RedirectToAction("Show");
        }
    }


}




     