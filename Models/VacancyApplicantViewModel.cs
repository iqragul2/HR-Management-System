namespace HRGroup.Models
{
    public class VacancyApplicantViewModel
    {
        public int ApplicantVacancyId { get; set; }
        public string ApplicantId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string VacancyId { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public string Status { get; set; }
        public DateTime? AttachedDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public int? InterviewId { get; set; }
        public DateTime? InterviewDate { get; set; }

    }
}
