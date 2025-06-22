using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using HRGroup.Data;  // apne DbContext ka namespace yahan likhen
using Microsoft.AspNetCore.Http;

public class RequireProfileAttribute : ActionFilterAttribute
{
    private readonly RecruitmentDbContext _context;

    public RequireProfileAttribute(RecruitmentDbContext context)
    {
        _context = context;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userId = context.HttpContext.Session.GetInt32("UserId") ?? 0;

        var employee = _context.Employees.FirstOrDefault(e => e.UserId == userId);

        if (employee == null)
        {
            var controller = (Controller)context.Controller;

            controller.TempData["NotificationMessage"] = "Please create your profile first!";
            controller.TempData["NotificationType"] = "warning";

            // Redirect to HR Dashboard or Profile creation page
            context.Result = new RedirectToActionResult("Dashboard", "HR", null);
        }

        base.OnActionExecuting(context);
    }
}
