using HRGroup.Data;
using HRGroup.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRGroup.Controllers
{
    public class AdminController : Controller
    {
        private readonly RecruitmentDbContext db;

        public AdminController(RecruitmentDbContext db)
        {
            this.db = db;
        }
      
           private IActionResult CheckAdminSession()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var user = db.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) { return RedirectToAction("Login", "Home"); }

            if (user.RoleId == 1)
            {
                return null; 
            }

            var employee = db.Employees.FirstOrDefault(e => e.UserId == userId);
            if (employee == null)
            {
                return RedirectToAction("Login", "Home");
            }

            return null;
        }




        public IActionResult Dashboard()
        {
            var sessionCheck = CheckAdminSession();
            if (sessionCheck != null) return sessionCheck;

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var user = db.Users.FirstOrDefault(u => u.UserId == userId);

            // Full name bana lo
            string adminName = "";
            if (user != null)
            {
                adminName = $"{user.FirstName} {user.LastName}";
            }
            else
            {
                adminName = "Admin";
            }

            ViewBag.AdminName = adminName;

            return View();
        }


        //All Pending Recruiter List
        [HttpGet]
        public IActionResult RecruiterRequestStatus()
        {
            var sessionCheck = CheckAdminSession();
            if (sessionCheck != null) return sessionCheck;

            // Fetch requests with Department info, where status is Pending
            var requests = db.RecruiterRequests
                .Include(r => r.User)
                .Where(r => r.Status == "Pending")
                .ToList();

            // All departments for dropdown or display
            var departments = db.Departments.ToList();
            ViewBag.Departments = departments;

            return View(requests);
        }
     
        //PRocess Of Accepting Recruiters in Pending List
        [HttpPost]
        public IActionResult ProcessRecruiterRequest(RecruiterRequestViewModel model)
        {
            var sessionCheck = CheckAdminSession();
            if (sessionCheck != null) return sessionCheck;

            var request = db.RecruiterRequests.FirstOrDefault(r => r.RequestId == model.RequestId);
            if (request == null || request.Status != "Pending")
                return NotFound();

            request.Status = "Approved";
            request.DepartmentId = "D001";

            var user = db.Users.Find(request.UserId);
            if (user != null)
            {
                user.RoleId = 3;
            }
 
            db.SaveChanges();

            return RedirectToAction("RecruiterRequestStatus");
        }

        //PRocess Of  Rejecting Recruiters in Pending List

        [HttpPost]
        public IActionResult RejectRecruiter(int requestId)
        {
            var sessionCheck = CheckAdminSession();
            if (sessionCheck != null) return sessionCheck;

            var request = db.RecruiterRequests.FirstOrDefault(r => r.RequestId == requestId);
            if (request != null && request.Status == "Pending")
            {
                request.Status = "Rejected";
                db.SaveChanges();
            }

            return RedirectToAction("RecruiterRequestStatus");
        }

        //Showing List Of Selected Recruiters 
        [HttpGet]
        public IActionResult ApprovedRecruiterRequests()
        {
            var approvedRequests = db.RecruiterRequests
                .Where(r => r.Status == "Approved")
                .OrderByDescending(r => r.RequestDate)
                .ToList();

            return View(approvedRequests);
        }


        //Showing List Of Rejected Recruiters
        [HttpGet]
        public IActionResult RejectedRecruiterRequests()
        {

            var sessionCheck = CheckAdminSession();
            if (sessionCheck != null) return sessionCheck;

            var rejectedRequests = db.RecruiterRequests
                .Where(r => r.Status == "Rejected")
                .OrderByDescending(r => r.RequestDate)
                .ToList();

            return View(rejectedRequests);
        }
        [HttpPost]
        public IActionResult DeleteApprovedRequest(int requestId)
        {
            var sessionCheck = CheckAdminSession();
            if (sessionCheck != null) return sessionCheck;

            var request = db.RecruiterRequests.FirstOrDefault(r => r.RequestId == requestId);
            if (request == null)
            {
                return NotFound();
            }

            db.RecruiterRequests.Remove(request);
           db.SaveChanges();

            TempData["Success"] = "Approved recruiter request deleted successfully!";
            return RedirectToAction("ApprovedRecruiterRequests");
        }
        //Details Of All Recruiters
        [HttpGet]
        public IActionResult RecruiterRequestDetails(int id)
        {
            var sessionCheck = CheckAdminSession();
            if (sessionCheck != null) return sessionCheck;

            var request = db.RecruiterRequests.FirstOrDefault(r => r.RequestId == id);
            if (request == null) return NotFound();

            return View(request);
        }

        // Selected Recruiter Status Update
        [HttpGet]
        public IActionResult SelectedRequestStatus(int requestId)
        {
            var sessionCheck = CheckAdminSession();
            if (sessionCheck != null) return sessionCheck;

            var request = db.RecruiterRequests.FirstOrDefault(r => r.RequestId == requestId);
            if (request == null) return NotFound();

            // For example, if you want to pass a list of valid statuses to a dropdown:
            ViewBag.ValidStatuses = new[] { "Pending", "Approved", "Rejected" };

            return View(request);
        }
        [HttpPost]
        public IActionResult SelectedRequestStatus(int requestId, string status)
        {
            var sessionCheck = CheckAdminSession();
            if (sessionCheck != null) return sessionCheck;

            var request = db.RecruiterRequests.FirstOrDefault(r => r.RequestId == requestId);
            if (request == null) return NotFound();

            var validStatuses = new[] { "Pending", "Approved", "Rejected" };
            if (!validStatuses.Contains(status))
            {
                ModelState.AddModelError("", "Invalid status value.");
                return RedirectToAction("RecruiterRequestsList");
            }


            if (request.Status == "Approved" && (status == "Pending" || status == "Rejected"))
            {
                var employee = db.Employees.FirstOrDefault(e => e.UserId == request.UserId);
                if (employee != null)
                {
                 
                    if (employee.Designation == "recruiter")
                    {
                        employee.Designation = null;  
                    }

                    var hrdDept = db.Departments.FirstOrDefault(d => d.Name == "HRD");
                    if (hrdDept != null && employee.DepartmentId == hrdDept.DepartmentId)
                    {
                        employee.DepartmentId = null;
                    }

                }
            }
            request.Status = status;
            db.SaveChanges();

            TempData["Success"] = "Request status updated successfully.";

            if (status == "Pending")
            {
                return RedirectToAction("RecruiterRequestStatus");
            }
            else if (status == "Approved")
            {
                return RedirectToAction("SelectedRecruiterRequests");
            }
            else if (status == "Rejected")
            {
                return RedirectToAction("RejectedRecruiterRequests");
            }

            return RedirectToAction("SelectedRecruiterRequests");
        }

        // Rejected Recruiter Status Update
        [HttpGet]
        public IActionResult RejectedRequestStatus(int requestId)
        {
            var sessionCheck = CheckAdminSession();
            if (sessionCheck != null) return sessionCheck;

            var request = db.RecruiterRequests.FirstOrDefault(r => r.RequestId == requestId);
            if (request == null) return NotFound();

            ViewBag.ValidStatuses = new[] { "Pending", "Approved", "Rejected" };

            return View(request);
        }
        [HttpPost]
        public IActionResult RejectedRequestStatus(int requestId, string status)
        {
            var sessionCheck = CheckAdminSession();
            if (sessionCheck != null) return sessionCheck;

            var request = db.RecruiterRequests.FirstOrDefault(r => r.RequestId == requestId);
            if (request == null) return NotFound();

            var validStatuses = new[] { "Pending", "Approved", "Rejected" };
            if (!validStatuses.Contains(status))
            {
                ModelState.AddModelError("", "Invalid status value.");
                return RedirectToAction("RecruiterRequestsList");
            }

            request.Status = status;
            db.SaveChanges();

            TempData["Success"] = "Request status updated successfully.";
            if (status == "Pending")
            {
                return RedirectToAction("RecruiterRequestStatus");
            }
            else if (status == "Approved")
            {
                return RedirectToAction("SelectedRecruiterRequests");
            }
            else if (status == "Rejected")
            {
                return RedirectToAction("RejectedRecruiterRequests");
            }
            return RedirectToAction("ApprovedRecruiterRequests");
        }


        //Add Departments
        [HttpGet]
        public IActionResult AddDepartment()
        {
            var sessionCheck = CheckAdminSession();
            if (sessionCheck != null) return sessionCheck;

            var lastDept = db.Departments.OrderByDescending(d => d.DepartmentId).FirstOrDefault();
            int nextId = 1;

            if (lastDept != null && lastDept.DepartmentId.Length > 1 &&
                int.TryParse(lastDept.DepartmentId.Substring(1), out int lastNumericId))
            {
                nextId = lastNumericId + 1;
            }

            var newDeptId = "D" + nextId.ToString("D3");

            var model = new Department
            {
                DepartmentId = newDeptId
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult AddDepartment(Department model, IFormFile imageFile)
        {
            var sessionCheck = CheckAdminSession();
            if (sessionCheck != null) return sessionCheck;

            if (ModelState.IsValid)
            {
                var lastDept = db.Departments
                    .OrderByDescending(d => d.DepartmentId)
                    .FirstOrDefault();

                int nextId = 1;

                if (lastDept != null && lastDept.DepartmentId.Length > 1 &&
                    int.TryParse(lastDept.DepartmentId.Substring(1), out int lastNumericId))
                {
                    nextId = lastNumericId + 1;
                }

                model.DepartmentId = "D" + nextId.ToString("D3");

                // Save image if provided
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/departments", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    model.ImageUrl = "/images/departments/" + fileName;
                }

                db.Departments.Add(model);
                db.SaveChanges();

                return RedirectToAction("ListDepartments");
            }

            return View(model);
        }
        [HttpGet]
        //List DEepartments
        public IActionResult ListDepartments()
        {
            var sessionCheck = CheckAdminSession();
            if (sessionCheck != null) return sessionCheck;

            var departments = db.Departments.ToList();
            return View(departments);
        }
        [HttpGet]
        public IActionResult EditDepartment(string id)
        {
            var sessionCheck = CheckAdminSession();
            if (sessionCheck != null) return sessionCheck;

            if (string.IsNullOrEmpty(id))
                return NotFound();

            var dept = db.Departments.FirstOrDefault(d => d.DepartmentId == id);
            if (dept == null)
                return NotFound();

            return View(dept);
        }

        [HttpPost]
        public IActionResult EditDepartment(Department model, IFormFile ImageFile)
        {
            var sessionCheck = CheckAdminSession();
            if (sessionCheck != null) return sessionCheck;

            var dept = db.Departments.FirstOrDefault(d => d.DepartmentId == model.DepartmentId);
            if (dept == null) return NotFound();

            dept.Name = model.Name;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/departments", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }
                dept.ImageUrl = "/images/departments/" + fileName;
            }

            db.Departments.Update(dept);
            db.SaveChanges();
            return RedirectToAction("ListDepartments");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
           
            return RedirectToAction("Login", "Home");
        }


    }
}
