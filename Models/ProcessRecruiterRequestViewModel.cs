namespace HRGroup.Models
{
    public class ProcessRecruiterRequestViewModel
    {
        public int RequestId { get; set; }
        public string Name { get; set; }
        public string Reason { get; set; }

        public string ContactInfo { get; set; }
        public string DepartmentId { get; set; }

        public List<Department> Departments { get; set; }
    }

}
