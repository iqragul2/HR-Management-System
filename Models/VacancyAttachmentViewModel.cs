namespace HRGroup.Models
{
    public class VacancyAttachmentViewModel
    {
        public int ApplicantVacancyId { get; set; }  
        public string ApplicantId { get; set; } = null!;
        public string VacancyId { get; set; } = null!;
        public string Title { get; set; }
        public string Department { get; set; }
        public string Status { get; set; }
        public DateTime? InterviewDate { get; set; }
        public int? InterviewId { get; set; } 
    }
}
