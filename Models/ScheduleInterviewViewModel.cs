namespace HRGroup.Models
{
    public class ScheduleInterviewViewModel
    {

      
        public DateTime ScheduledDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Notes { get; set; } 
        public string Title { get; set; } = string.Empty;

        public int InterviewId { get; set; }
        // Applicant Info
        public int ApplicantVacancyId { get; set; }
        public string ApplicantId { get; set; }
        public string ApplicantName { get; set; }
        public string ApplicantImageUrl { get; set; }
        public string ApplicantEmail { get; set; } = string.Empty;


        // Vacancy Info
        public string VacancyTitle { get; set; }
        public string DepartmentName { get; set; }

        // Interviewer Info (Employee)
        public string InterviewerName { get; set; } = string.Empty;
        public string InterviewerImage { get; set; }
        public int InterviewerId { get; set; }
        public List<Employee> Interviewers { get; set; } = new();

        public DateTime? InterviewScheduledDate { get; set; }
        public string InterviewerDepartment { get; set; }
        public string InterviewerDesignation { get; set; }
        public string Result { get; set; }

    }
}
