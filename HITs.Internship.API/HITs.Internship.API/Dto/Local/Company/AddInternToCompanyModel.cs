using System.ComponentModel.DataAnnotations;

namespace HITs.Internship.API.Dto.Local.Company
{
    public class AddInternToCompanyModel
    {
        [Required]
        public int InternId { get; set; }
        [Required]
        public int Semester { get; set; }
    }
}
