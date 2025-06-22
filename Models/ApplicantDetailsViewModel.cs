namespace HRGroup.Models
{
    public class ApplicantDetailsViewModel
    {
        public Applicant Applicant { get; set; }
        public List<VacancyAttachmentViewModel> AttachedVacancies { get; set; }
        public IEnumerable<Department> Departments { get; set; } = new List<Department>();


    }
}
