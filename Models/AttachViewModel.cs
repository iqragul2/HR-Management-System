using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRGroup.Models
{
    public class AttachViewModel
    {
   
    public Applicant Applicant { get; set; } 
        public string ApplicantId { get; set; }
        public string ApplicantName { get; set; }
        public string VacancyId { get; set; }
        public List<Vacancy> Vacancies { get; set; }
        public SelectList VacancySelectList { get; set; }
    }
}
