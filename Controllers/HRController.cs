using HRGroup.Data;
using HRGroup.Helpers;
using HRGroup.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;



namespace HRGroup.Controllers
{
    
    public class HRController : Controller
    {
        private readonly RecruitmentDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly EmailHelper _emailHelper;

        public HRController(RecruitmentDbContext context, IWebHostEnvironment env, EmailHelper emailHelper)
        {
            _context = context;
            _env = env;
            _emailHelper = emailHelper;
        }

        private void CloseExpiredVacancies()
        {
            var expiredVacancies = _context.Vacancies
                .Where(v => v.ClosingDate <= DateTime.Today && v.Status == "Open")
                .ToList();

            foreach (var vacancy in expiredVacancies)
            {
                vacancy.Status = "Close";
            }

            _context.SaveChanges();
        }


        // ================================
        // ======= AutoClose Vacancy  =====
        // ================================


        private void AutoCloseVacancyIfFilled(string vacancyId)
        {
            var vacancy = _context.Vacancies.FirstOrDefault(v => v.VacancyId == vacancyId);

            if (vacancy == null)
                return;

            int selectedCount = _context.ApplicantVacancies
                .Count(av => av.VacancyId == vacancyId && av.Status == "Selected");

            if (selectedCount >= vacancy.Openings)
            {
                vacancy.Status = "Close";
                _context.SaveChanges();
            }
        }

     
        // ================================================================
        // ======= GEnerate VAcancy ID, Employee ID And Applicants ID =====
        // ================================================================
      
        private string GenerateVacancyId()
        {
            var lastVacancy = _context.Vacancies
                .OrderByDescending(v => v.VacancyId)
                .FirstOrDefault();

            if (lastVacancy == null)
                return "V0001";

            int lastNumber = int.Parse(lastVacancy.VacancyId.Substring(1));
            return $"V{(lastNumber + 1).ToString("D4")}";
        }
        private int GetCurrentHrEmployeeId()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var employee = _context.Employees.FirstOrDefault(e => e.UserId == userId);
            return employee?.EmployeeId ?? 0;
        }
        private string GenerateApplicantId()
        {
            var lastApplicant = _context.Applicants
                .OrderByDescending(a => a.ApplicantId)
                .FirstOrDefault();

            if (lastApplicant == null)
                return "A0001";

            int lastNum = int.Parse(lastApplicant.ApplicantId.Substring(1));
            return $"A{(lastNum + 1).ToString("D4")}";
        }


        // ============================
        // ======= HR Dashboard =======
        // ============================

        [HttpGet]
        public IActionResult Dashboard()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user != null && user.RoleId == 3)
            {
                // Find the corresponding employee
                var employee = _context.Employees.FirstOrDefault(e => e.UserId == userId);

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
                // If user is not RoleId=2
                return RedirectToAction("AccessDenied", "Home");
            }

            return View();
        }



        // ==============================================
        // ======= Create,Update And Show Profile =======
        // ==============================================




        /* [HttpPost]
                public IActionResult Create(Employee employee, IFormFile ProfileImage)
                {
                    int loggedInUserId = 0;
                    int loggedInUserRoleId = 0;

                    if (HttpContext.Session.GetInt32("UserId") != null)
                    {
                        loggedInUserId = (int)HttpContext.Session.GetInt32("UserId");
                    }

                    if (HttpContext.Session.GetInt32("RoleId") != null)
                    {
                        loggedInUserRoleId = (int)HttpContext.Session.GetInt32("RoleId");
                    }

                    if (loggedInUserRoleId == 3)
                    {
                        employee.Designation = "HR Manager";
                        employee.DepartmentId = "D001";

                        if (ProfileImage != null)
                        {
                            var filePath = Path.Combine("wwwroot/images", ProfileImage.FileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                ProfileImage.CopyTo(stream);
                            }
                            employee.ProfileImageUrl = "/images/" + ProfileImage.FileName;
                        }

                        employee.UserId = loggedInUserId;

                        _context.Employees.Add(employee);
                        _context.SaveChanges();

                    }
                    return RedirectToAction("Show", "HR");

                }*/
        public IActionResult Show()
        {
            // Get logged-in user's UserId
            int loggedInUserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // Fetch employee record
            var employee = _context.Employees.FirstOrDefault(e => e.UserId == loggedInUserId);
            if (employee == null)
            {
                return Content("No profile found.");
            }

            // Fetch roleId directly from DB (no need to get from session)
            var user = _context.Users.FirstOrDefault(u => u.UserId == loggedInUserId);
            int roleId = user?.RoleId ?? 0; // If user not found, 0 (no role)

            ViewBag.RoleId = roleId;

            return View(employee);
        }

        [HttpGet]
        public IActionResult Create()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // Fetch role directly from DB instead of session
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null || user.RoleId != 3)
            {
                return Content("Unauthorized to create employee.");
            }

            var employee = new Employee
            {
                Designation = "HR Manager",
                DepartmentId = "D001"
            };
            return View(employee);
        }

        [HttpPost]
        public IActionResult Create(Employee employee, IFormFile ProfileImage)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // Fetch role directly from DB instead of session
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null || user.RoleId != 3)
            {
                return Content("Unauthorized to create employee.");
            }

            // Default values
            employee.Designation = "HR Manager";
            employee.DepartmentId = "D001";

            // Find applicant by email
            var applicant = _context.Applicants.FirstOrDefault(a => a.Email == user.Email);
            if (applicant != null)
            {
                var hiredVacancy = _context.ApplicantVacancies
                    .FirstOrDefault(av => av.ApplicantId == applicant.ApplicantId && av.Status == "Selected");

                if (hiredVacancy != null)
                {
                    var vacancy = _context.Vacancies.FirstOrDefault(v => v.VacancyId == hiredVacancy.VacancyId);

                    if (vacancy != null)
                    {
                        employee.DepartmentId = vacancy.DepartmentId;
                        employee.Designation = "Employee";
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

            employee.UserId = userId;

            _context.Employees.Add(employee);
            _context.SaveChanges();

            return RedirectToAction("Show");
        }

        [HttpGet]
        public IActionResult Edit(int employeeId)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);

            if (employee == null)
            {
                return Content("Profile not found.");
            }

            // Get logged-in user's UserId
            int loggedInUserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var user = _context.Users.FirstOrDefault(u => u.UserId == loggedInUserId);
            int roleId = user?.RoleId ?? 0; 

            ViewBag.RoleId = roleId;

            return View(employee);
        }


        [HttpPost]
        public IActionResult Edit(Employee updatedEmployee, IFormFile ProfileImage)
        {
            var existingEmployee = _context.Employees.FirstOrDefault(e => e.EmployeeId == updatedEmployee.EmployeeId);

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

            // HR ka role wapas set na karein edit pe
            if (existingEmployee.Designation != "Recruiter")
            {
                existingEmployee.Designation = updatedEmployee.Designation;
                existingEmployee.DepartmentId = updatedEmployee.DepartmentId;
            }

            if (ProfileImage != null)
            {
                var filePath = Path.Combine("wwwroot/images", ProfileImage.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ProfileImage.CopyTo(stream);
                }
                existingEmployee.ProfileImageUrl = "/images/" + ProfileImage.FileName;
            }

            _context.SaveChanges();

            return RedirectToAction("Show");
        }


        // ==================================================================
        // ======= Create And Update Vacancies Also List Of Vacancies =======
        // ==================================================================
      
        
        [HttpGet]
        [ServiceFilter(typeof(RequireProfileAttribute))]
        public IActionResult CreateVacancy(string departmentId)
        {
            var currentHrId = GetCurrentHrEmployeeId();

            if (currentHrId == 0)
            {
                TempData["NotificationMessage"] = "Please login first.";
                TempData["NotificationType"] = "warning";
                return RedirectToAction("Login", "Home");
            }

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var employee = _context.Employees.FirstOrDefault(e => e.UserId == userId);
            if (employee == null)
            {
                TempData["NotificationMessage"] = "Please create your profile first before creating a vacancy!";
                TempData["NotificationType"] = "warning";
                return RedirectToAction("Dashboard", "HR");
            }

            var allDepartments = _context.Departments.ToList();

            var model = new VacancyCreateViewModel
            {
                Vacancy = new Vacancy
                {
                    VacancyId = GenerateVacancyId(),
                    CreatedBy = currentHrId,
                    CreatedDate = DateTime.Now,
                    Status = "Open",
                    DepartmentId = departmentId  
                },
                Departments = allDepartments
            };

            return View(model);
        }


        [HttpPost]
        public IActionResult CreateVacancy(VacancyCreateViewModel model)
        {
            CloseExpiredVacancies();
            var currentHrId = GetCurrentHrEmployeeId();

            ModelState.Remove("Vacancy.CreatedByNavigation");
            ModelState.Remove("Vacancy.Department");

            if (ModelState.IsValid)
            {
                model.Vacancy.VacancyId = GenerateVacancyId();
                model.Vacancy.CreatedBy = currentHrId;
                model.Vacancy.CreatedDate = DateTime.Now;

                if (string.IsNullOrEmpty(model.Vacancy.Status))
                    model.Vacancy.Status = "Open";

                if (model.Vacancy.ClosingDate.HasValue && model.Vacancy.ClosingDate.Value.Date <= DateTime.Today)
                {
                    model.Vacancy.Status = "Close";
                }

                _context.Vacancies.Add(model.Vacancy);
                _context.SaveChanges();

                return RedirectToAction("Vacancies");
            }

            model.Departments = _context.Departments.ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult EditVacancy(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var vacancy =  _context.Vacancies.Find(id);
            if (vacancy == null)
                return NotFound();

            var currentHrId = GetCurrentHrEmployeeId();
            var currentHr = _context.Employees.FirstOrDefault(e => e.EmployeeId == currentHrId);

            var allDepartments = _context.Departments.ToList();

        
            var model = new VacancyCreateViewModel
            {
                Vacancy = vacancy,
                Departments = allDepartments
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult EditVacancy(string id, VacancyCreateViewModel model)
        {
            if (id != model.Vacancy.VacancyId)
                return NotFound();

            var currentHrId = GetCurrentHrEmployeeId();
            var currentHr = _context.Employees.FirstOrDefault(e => e.EmployeeId == currentHrId);

            // Validate department
            var deptExists = _context.Departments.Any(d => d.DepartmentId == model.Vacancy.DepartmentId);
            if (!deptExists)
            {
                ModelState.AddModelError("DepartmentId", "Selected department does not exist.");
                model.Departments = _context.Departments.ToList();
                return View(model);
            }

            // Validate ClosingDate is not in the past
            if (model.Vacancy.ClosingDate.HasValue && model.Vacancy.ClosingDate.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError("Vacancy.ClosingDate", "Closing date cannot be in the past.");
                model.Departments = _context.Departments.ToList();
                return View(model);
            }

            ModelState.Remove("Vacancy.CreatedByNavigation");
            ModelState.Remove("Vacancy.Department");

            if (ModelState.IsValid)
            {
                var vacancy = _context.Vacancies.Find(id);
                if (vacancy == null)
                    return NotFound();

                // Business rules
                if (vacancy.Status == "Close" && (model.Vacancy.Status == "Open" || model.Vacancy.Status == "Suspended"))
                {
                    ModelState.AddModelError("Vacancy.Status", "Closed vacancy cannot be reopened or suspended.");
                    model.Departments = _context.Departments.ToList();
                    return View(model);
                }

                if (vacancy.Status == "Open" && (model.Vacancy.Status == "Close" || model.Vacancy.Status == "Suspended"))
                {
                    vacancy.Status = model.Vacancy.Status;
                }

                if (vacancy.Status == "Suspended" && (model.Vacancy.Status == "Open" || model.Vacancy.Status == "Close"))
                {
                    vacancy.Status = model.Vacancy.Status;
                }

                // Update fields
                vacancy.Title = model.Vacancy.Title;
                vacancy.JobDescription = model.Vacancy.JobDescription;
                vacancy.DepartmentId = model.Vacancy.DepartmentId;
                vacancy.Status = model.Vacancy.Status;
                vacancy.Openings = model.Vacancy.Openings;
                vacancy.ClosingDate = model.Vacancy.ClosingDate;
                vacancy.CreatedBy = model.Vacancy.CreatedBy;

                _context.Vacancies.Update(vacancy);
                _context.SaveChanges();

                return RedirectToAction("Vacancies");
            }

            model.Departments = _context.Departments.ToList();
            return View(model);
        }

        [HttpGet]
        public IActionResult Vacancies()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var employee = _context.Employees.FirstOrDefault(e => e.UserId == userId);
            if (employee == null) return RedirectToAction("Login", "Home");

            var vacancies = _context.Vacancies.Where(v => v.CreatedBy == employee.EmployeeId).Include(v => v.Department).ToList();

            var departmentDict = _context.Departments.ToDictionary(d => d.DepartmentId, d => d);


            return View(vacancies);
        }
   
   
        // =================================================================
        // ======= Create And Update Applicants Also List Applicants  ======
        // =================================================================
     
        
        [HttpGet]
        [ServiceFilter(typeof(RequireProfileAttribute))]
        public IActionResult CreateApplicant()
        {
            var model = new Applicant
            {
                ApplicantId = GenerateApplicantId(),
                AppliedDate = DateTime.Now,
                Status = "Not in Process"
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult CreateApplicant(Applicant model)
        {
            if (ModelState.IsValid)
            {
                model.ApplicantId = GenerateApplicantId();
                model.AppliedDate = DateTime.Now; // enforce date
                model.Status = "Not in Process";  // enforce initial status

                _context.Applicants.Add(model);
                _context.SaveChanges();

                return RedirectToAction("ListApplicants"); // List all applicants
            }

            return View(model);
        }
        [HttpGet]
        public IActionResult UpdateApplicant(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var applicant = _context.Applicants.Find(id);
            if (applicant == null)
                return NotFound();

            return View(applicant); // Applicant model view me pass hoga
        }

        [HttpPost]
        public IActionResult UpdateApplicant(Applicant model)
        {
            if (ModelState.IsValid)
            {
                var existingApplicant = _context.Applicants.Find(model.ApplicantId);
                if (existingApplicant == null)
                    return NotFound();

                // Only update allowed fields
                existingApplicant.Name = model.Name;
                existingApplicant.Email = model.Email;
                existingApplicant.PhoneNumber = model.PhoneNumber;

                _context.Applicants.Update(existingApplicant);
                _context.SaveChanges();

                return RedirectToAction("ListApplicants"); // back to list
            }

            return View(model);
        }

        [HttpGet]
        [ServiceFilter(typeof(RequireProfileAttribute))]
        public IActionResult ListApplicants()
        {
            var applicants = _context.Applicants.ToList();
            return View(applicants);
        }


        // =========================================================================
        // ======= Attach Applicant To Vacancy And List Of Attach Applicants =======
        // =========================================================================


        [HttpGet]
        public IActionResult AttachToVacancy(string applicantId)
        {
            if (string.IsNullOrEmpty(applicantId))
                return NotFound();

            var applicant = _context.Applicants.Find(applicantId);
            if (applicant == null)
                return NotFound();

            // Prevent attaching if status is Hired or Banned
            if (applicant.Status == "Hired" || applicant.Status == "Banned")
            {
                ModelState.AddModelError("", "This applicant cannot be attached to any vacancy because their status is " + applicant.Status);
                return View("Error"); // or some error page/view
            }

            // Pass applicant & vacancies to the view to select vacancy
            var vacancies = _context.Vacancies.Where(v => v.Status == "Open").ToList();

            var model = new AttachViewModel
            {
                Applicant = applicant,
                Vacancies = vacancies
            };

            return View(model);
        }
        [HttpPost]
        public IActionResult AttachToVacancy(AttachViewModel model)
        {
            if (string.IsNullOrEmpty(model.ApplicantId) || string.IsNullOrEmpty(model.VacancyId))
                return NotFound();

            var applicant = _context.Applicants.Find(model.ApplicantId);
            if (applicant == null)
                return NotFound();

            if (applicant.Status == "Hired" || applicant.Status == "Banned")
            {
                ModelState.AddModelError("", "This applicant cannot be attached to any vacancy because their status is " + applicant.Status);
                return View("Error");
            }

            // Check if already attached to this vacancy
            bool alreadyAttached = _context.ApplicantVacancies.Any(av => av.ApplicantId == model.ApplicantId && av.VacancyId == model.VacancyId);
            if (alreadyAttached)
            {
                ModelState.AddModelError("", "Applicant is already attached to this vacancy.");
                return View("Error");
            }

            // Attach applicant to vacancy
            var applicantVacancy = new ApplicantVacancy
            {
                ApplicantId = model.ApplicantId,
                VacancyId = model.VacancyId,
                Status = "Not Scheduled" // or initial status
            };
            _context.ApplicantVacancies.Add(applicantVacancy);

            // Update applicant status to In Process
            applicant.Status = "In Process";
            _context.Applicants.Update(applicant);

            _context.SaveChanges();

            return RedirectToAction("AllVacancyApplicants", new { id = model.ApplicantId });
        }

        [HttpGet]
        public IActionResult AllVacancyApplicants()
        {
            var data = _context.ApplicantVacancies
                .Join(_context.Applicants,
                    av => av.ApplicantId,
                    a => a.ApplicantId,
                    (av, a) => new { av, a })
                .Join(_context.Vacancies,
                    combined => combined.av.VacancyId,
                    v => v.VacancyId,
                    (combined, v) => new VacancyApplicantViewModel
                    {
                        ApplicantVacancyId = combined.av.Id,
                        ApplicantId = combined.a.ApplicantId,
                        Name = combined.a.Name,
                        Email = combined.a.Email,
                        PhoneNumber = combined.a.PhoneNumber,
                        VacancyId = v.VacancyId,
                        Title = v.Title,
                        Department = v.Department.Name, 
                        Status = combined.av.Status,
                        AttachedDate = combined.av.AttachedDate,
                        ScheduledDate = combined.av.InterviewScheduledDate
                    })
                .OrderByDescending(x => x.AttachedDate)
                .ToList();

            return View(data);
        }
        [HttpGet]
        public IActionResult AttachApplicantVacancyDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var applicant = _context.Applicants.FirstOrDefault(a => a.ApplicantId == id);
            if (applicant == null)
                return NotFound();

            // Get all attached vacancies for this applicant (usually 1)
            var attachedVacancies = _context.ApplicantVacancies
                .Where(av => av.ApplicantId == id)
             .Join(_context.Vacancies.Include(v => v.Department),av => av.VacancyId,v => v.VacancyId,(av, v) => new{av.Id,av.VacancyId,Vacancy = v,av.Status,av.InterviewScheduledDate}).ToList();

            var model = new ApplicantDetailsViewModel
            {
                Applicant = applicant,
                AttachedVacancies = attachedVacancies.Select(x => new VacancyAttachmentViewModel
                {
                    ApplicantVacancyId = x.Id, // Now available
                    VacancyId = x.VacancyId,
                    Title = x.Vacancy.Title,
                    Department = x.Vacancy.Department.Name,
                    Status = x.Status,
                    InterviewDate = x.InterviewScheduledDate
                }).ToList()

            };

            return View(model);
        }



        // ================================================================================
        // ======= Schedule Interview,Update Interview And Schedule Interviews List =======
        // ================================================================================


        [HttpGet]
        public IActionResult ScheduleInterview(int applicantVacancyId)
        {
            var av = _context.ApplicantVacancies.FirstOrDefault(av => av.Id == applicantVacancyId);
            if (av == null) return NotFound();

            var applicant = _context.Applicants.FirstOrDefault(a => a.ApplicantId == av.ApplicantId);
            var vacancy = _context.Vacancies.FirstOrDefault(v => v.VacancyId == av.VacancyId);
            if (vacancy == null || applicant == null)
                return NotFound();

            int? loggedInEmployeeId = HttpContext.Session.GetInt32("EmployeeId");
            var loggedInEmployee = _context.Employees
                .Include(e => e.User)
                .Include(e => e.Department)
                .FirstOrDefault(e => e.EmployeeId == loggedInEmployeeId);

            if (loggedInEmployee == null)
                return NotFound("Logged-in employee not found.");

            var model = new ScheduleInterviewViewModel
            {
                ApplicantVacancyId = av.Id,
                ApplicantName = applicant.Name,
                Title = vacancy.Title,

                // Remove dropdown, directly set InterviewerId to logged-in user
                InterviewerId = loggedInEmployee.EmployeeId,

                ScheduledDate = DateTime.Today.AddDays(1),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(11),

                InterviewerName = loggedInEmployee.FullName,
                InterviewerDesignation = loggedInEmployee.Designation,
                InterviewerDepartment = loggedInEmployee.Department?.Name ?? "Unknown"
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ScheduleInterview(ScheduleInterviewViewModel model)
        {
            if (model.ScheduledDate < DateTime.Today)
            {
                ModelState.AddModelError("", "Interview date must be in the future.");
                return View(model);
            }

            var av = _context.ApplicantVacancies.Find(model.ApplicantVacancyId);
            if (av == null) return NotFound();

            var vacancy = _context.Vacancies.Find(av.VacancyId);

            // Automatically set interviewer to logged-in user
            int? loggedInEmployeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (loggedInEmployeeId == null)
                return Content("Session missing: EmployeeId not set.");

            var interviewer = _context.Employees
                .Include(e => e.User)
                .Include(e => e.Department)
                .FirstOrDefault(e => e.EmployeeId == loggedInEmployeeId);

            if (vacancy == null || interviewer == null)
            {
                ModelState.AddModelError("", "Invalid vacancy or interviewer.");
                return View(model);
            }

         

            var interviewStart = model.ScheduledDate + model.StartTime;
            var interviewEnd = model.ScheduledDate + model.EndTime;

            // Conflict check
            var existingInterviews = _context.Interviews
                .Where(i => i.InterviewerId == interviewer.EmployeeId && i.ScheduledDate.Date == model.ScheduledDate.Date)
                .ToList();

            foreach (var i in existingInterviews)
            {
                var existingStart = i.ScheduledDate;
                var existingEnd = i.ScheduledDate.AddHours(1); // or your EndTime

                if (interviewStart < existingEnd && interviewEnd > existingStart)
                {
                    ModelState.AddModelError("", "You have a conflicting interview at that time.");
                    return View(model);
                }
            }

            var interview = new Interview
            {
                Result = "Interview Scheduled",
                ApplicantVacancyId = model.ApplicantVacancyId,
                InterviewerId = interviewer.EmployeeId,
                ScheduledDate = interviewStart,
                Notes = model.Notes
            };

            _context.Interviews.Add(interview);

            av.Status = "Interview Scheduled";
            av.IsInterviewScheduled = true;
            av.InterviewerId = interviewer.EmployeeId;
            av.InterviewScheduledDate = interviewStart;

            _context.SaveChanges();

            if (interviewer.User != null && !string.IsNullOrEmpty(interviewer.User.Email))
            {
                _emailHelper.SendEmail(
                    interviewer.User.Email,
                    "Interview Scheduled",
                    $"Dear {interviewer.FullName},\n\nYou have been scheduled to conduct an interview for '{model.Title}' on {interview.ScheduledDate:dddd, MMMM dd yyyy, hh:mm tt}.\n\nRegards,\nHR System"
                );
            }


            // Send email notification to applicant
            if (av.Applicant != null && !string.IsNullOrEmpty(av.Applicant.Email))
            {
                _emailHelper.SendEmail(
                    av.Applicant.Email,
                    "Interview Scheduled",
                    $"Dear {av.Applicant.Name},\n\nYour interview for the position '{vacancy.Title}' has been scheduled on {interviewStart:dddd, MMMM dd yyyy, hh:mm tt}.\n\nPlease be prepared.\n\nRegards,\nHR System"
                );
            }
            TempData["Success"] = "Interview scheduled and notifications sent.";
            return RedirectToAction("ListScheduledInterviews");
        }

        [HttpPost]
        public IActionResult UpdateInterviewResult(int interviewId, string result, string notes)
        {
            var interview = _context.Interviews
                .Include(i => i.ApplicantVacancy)
                    .ThenInclude(av => av.Applicant)
                .Include(i => i.ApplicantVacancy.Vacancy)
                .FirstOrDefault(i => i.InterviewId == interviewId);

            if (interview == null)
            {
                return NotFound();
            }

            // Update interview details
            interview.Result = result;
            interview.Notes = notes ?? "";

            var av = interview.ApplicantVacancy;
            if (av != null)
            {
                // Update ApplicantVacancy details
                av.Status = result;
                av.IsInterviewScheduled = true;
                av.InterviewScheduledDate = interview.ScheduledDate;
                av.InterviewerId = interview.InterviewerId;

                var applicant = av.Applicant;
                if (applicant != null)
                {
                    if (result == "Selected")
                    {
                        applicant.Status = "Hired";

                        // Extract numeric part of ApplicantId (e.g., "A0006" -> "6")
                        var numericPart = new string(applicant.ApplicantId.Where(char.IsDigit).ToArray());
                        if (int.TryParse(numericPart, out int applicantIdInt))
                        {
                            var employee = _context.Employees.FirstOrDefault(e => e.EmployeeId == applicantIdInt);

                            if (employee != null && av.Vacancy != null)
                            {
                                // ✅ Update employee's Designation with Vacancy Title
                                employee.Designation = av.Vacancy.Title;

                                // ✅ Update employee's DepartmentId directly
                                employee.DepartmentId = av.Vacancy.DepartmentId;
                            }
                        }

                        // ✅ Auto-close the vacancy if filled
                        AutoCloseVacancyIfFilled(av.VacancyId);
                    }
                    else if (result == "Rejected")
                    {
                        applicant.Status = "Banned";
                    }
                }
            }

            // Save changes to the database
            _context.SaveChanges();

            TempData["Success"] = "Interview result and applicant status updated successfully.";

            return RedirectToAction("ListApplicants");
        }


        [HttpGet]
        public IActionResult ListScheduledInterviews()
        {
            var interviews = _context.Interviews
                .Include(i => i.ApplicantVacancy)
                .ThenInclude(av => av.Applicant)
                .Include(i => i.ApplicantVacancy)
                .ThenInclude(av => av.Vacancy)
                .Include(i => i.Interviewer)
                .ThenInclude(emp => emp.Department)
                .Select(i => new ScheduleInterviewViewModel
                {
                    InterviewId = i.InterviewId,
                    ApplicantName = i.ApplicantVacancy.Applicant.Name,
                    Title = i.ApplicantVacancy.Vacancy.Title,
                    InterviewerName = i.Interviewer.FullName,
                    InterviewerDepartment = i.Interviewer.Department.Name,
                    InterviewerDesignation = i.Interviewer.Designation,
                    ScheduledDate = i.ScheduledDate,
                    Notes = i.Notes,
                    Result = i.Result 
                })
                .ToList();

            return View(interviews);
        }


        // ==========================================================================================
        // ======= Selected Applicant List After Interview and cancel interview After Schedule ======
        // ==========================================================================================


        [HttpGet]
        public IActionResult SelectedApplicants(int interviewerId)
        {
            var selectedApplicants = _context.Interviews
                .Include(i => i.ApplicantVacancy)
                    .ThenInclude(av => av.Applicant)
                .Where(i => i.InterviewerId == interviewerId && i.Result == "Selected")
                .Select(i => new
                {
                    ApplicantName = i.ApplicantVacancy.Applicant.Name,
                    VacancyTitle = i.ApplicantVacancy.Vacancy.Title,
                    InterviewDate = i.ScheduledDate
                })
                .ToList();

            return View(selectedApplicants);
        }


        [HttpPost]
        public IActionResult CancelInterview(int interviewId)
        {
            var interview = _context.Interviews.Find(interviewId);
            if (interview == null) return NotFound();

            var av = _context.ApplicantVacancies.Find(interview.ApplicantVacancyId);
            if (av != null)
            {
                av.IsInterviewScheduled = false;
                av.InterviewerId = null;
            }

            _context.Interviews.Remove(interview);
            _context.SaveChanges();

            return RedirectToAction("ListApplicants");
        }


        // ================================
        // ======= User Become Hr  ========
        // ================================


        [HttpGet]
        public IActionResult BecomeRecruiter()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Home");

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return RedirectToAction("Login", "Home");

            var model = new RecruiterRequest
            {
                Name = user.FirstName + " " + user.LastName,
                ContactInfo = user.Email
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult SubmitRecruiterRequest(RecruiterRequest model)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Home");

            model.UserId = userId.Value;
            model.RequestDate = DateTime.Now;
            model.Status = "Pending";

            _context.RecruiterRequests.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Your request has been submitted successfully!";
            return RedirectToAction("Dashboard","Home");
        }


        // ==================================================
        // ======= Show All Vacabcies With Departments ======
        // ==================================================


        [HttpGet]
        public IActionResult AllVacancies(string departmentId)
        {
            var departments = _context.Departments.AsNoTracking().ToList();

            List<Vacancy> vacancies;

            if (!string.IsNullOrEmpty(departmentId))
            {
                vacancies = _context.Vacancies
                    .AsNoTracking()
                    .Where(v => v.DepartmentId == departmentId && v.Status == "Open")
                    .ToList();
            }
            else
            {
                vacancies = _context.Vacancies
                    .AsNoTracking()
                    .Where(v => v.Status == "Open")
                    .ToList();
            }

            ViewBag.SelectedDepartmentId = departmentId ?? "";

            return View(Tuple.Create(departments, vacancies));
        }


        // ==================================================
        // ======= Search With Applicant And Vacancy ID =====
        // ==================================================


        [HttpGet]
        public IActionResult SearchResults(string query)
        {
            var vacancies = _context.Vacancies
                .Where(v => v.Title.Contains(query) || v.VacancyId.Contains(query))
                .ToList();

            var applicants = _context.Applicants
                .Where(a => a.Name.Contains(query) || a.ApplicantId.Contains(query))
                .ToList();

          
                ViewBag.SearchQuery = query;
                ViewBag.Vacancies = vacancies;
                ViewBag.Applicants = applicants;
            

            return View();
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }


    }
}
