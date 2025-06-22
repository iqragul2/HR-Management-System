namespace HRGroup.Models
{
  
        public class DashboardViewModel
        {

            // Basic HR & Employee info
            public string HRName { get; set; }
            public Employee Employee { get; set; }

            // Counts
            public int VacancyCount { get; set; }
            public int InterviewCount { get; set; }
            public int ApplicantCount { get; set; }

            // Growth percentages
            public double VacancyGrowth { get; set; }
            public double InterviewGrowth { get; set; }
            public double ApplicantGrowth { get; set; }

            // Same department stats
            public int SameDepartmentEmployeeCount { get; set; }
            public int SameDepartmentVacancyCount { get; set; }

            // Interviews assigned to user
            public List<ScheduleInterviewViewModel> UserInterviews { get; set; }

            // Recent search history
            public List<string> RecentSearches { get; set; }

            // Search term from user input
            public string SearchTerm { get; set; }

            // Search results
            public List<Employee> Employees { get; set; }
            public List<Vacancy> Vacancies { get; set; }
            public List<Applicant> Applicants { get; set; }

            // Activities timeline
            public List<ActivityViewModel> Activities { get; set; } = new List<ActivityViewModel>();

        // 🟢 Add these:
        public List<ScheduleInterviewViewModel> PagedInterviews { get; set; }
        public int InterviewCurrentPage { get; set; }
        public int InterviewTotalPages { get; set; }

   
        // Pagination properties
        public int CurrentPage { get; set; }
            public int TotalPages { get; set; }
        }

        // Timeline Activity item
        public class ActivityViewModel
        {
            public string Time { get; set; }            // e.g., 14:35
            public string Status { get; set; }          // e.g., primary, success, warning, etc.
            public string Content { get; set; }         // e.g., "New applicant registered: Ali"
        }


    }




