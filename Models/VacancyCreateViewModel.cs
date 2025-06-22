using System.ComponentModel.DataAnnotations;

namespace HRGroup.Models
{
    public class VacancyCreateViewModel
    {
        public Vacancy Vacancy { get; set; } = new Vacancy();

        public IEnumerable<Department> Departments { get; set; } = new List<Department>();


        [Required(ErrorMessage = "Department is required.")]
        public string DepartmentId
        {
            get => Vacancy.DepartmentId;
            set => Vacancy.DepartmentId = value;
        }
    }

}
