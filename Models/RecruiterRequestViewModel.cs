using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRGroup.Models
{
    public class RecruiterRequestViewModel
    {

        public int UserId { get; set; }

        public int RequestId { get; set; }

        public string Name { get; set; }

      
        public string ContactInfo { get; set; }

        public string Reason { get; set; }

        public string DepartmentId { get; set; }
        public DateTime JoinedDate { get; set; } = DateTime.Now;
        public RecruiterRequest Request { get; set; } = new RecruiterRequest();
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
    }


}
