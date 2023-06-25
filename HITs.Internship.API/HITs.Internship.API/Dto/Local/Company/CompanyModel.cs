using HITs.Internship.API.Dto.Local.Internship;
using HITs.Internship.API.Dto.UsersService;

namespace HITs.Internship.API.Dto.Local.Company
{
    public class CompanyModel : CompanyDto
    {
        public List<InternshipPreview> Internships { get; set; }
        public CompanyModel(CompanyDto companyDto, List<InternshipPreview> interns)
        {
            Id = companyDto.Id;
            Name = companyDto.Name;
            OfficialName = companyDto.OfficialName;
            Contacts = companyDto.Contacts;
            Representative = companyDto.Representative;

            Internships = interns;
        }
    }
}
