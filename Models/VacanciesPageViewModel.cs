

namespace HRGroup.Models
{
    public class VacanciesPageViewModel
    {
        public List<Department> Departments { get; set; } = new List<Department>();
        public List<Vacancy> Vacancies { get; set; } = new List<Vacancy>();

        // Applicant info for the modal
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
