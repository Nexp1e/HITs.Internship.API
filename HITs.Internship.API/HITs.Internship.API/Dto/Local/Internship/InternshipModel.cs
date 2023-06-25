using HITs.Internship.API.Dto.UsersService;

namespace HITs.Internship.API.Dto.Local.Internship
{
    public class InternshipModel
    {
        public int Id { get; set; }
        public int Semester { get; set; }
        public int StudyYear { get; set; }
        public double? Mark { get; set; }
        public string? Characteristic { get; set; }

        public List<DiaryCommentModel> DiaryComments { get; set; }
        public CompanyDto Company { get; set; }
        public UserDto Intern { get; set; }

        public InternshipModel() {}

        public InternshipModel(Data.Internship internship, CompanyDto company, UserDto user)
        {
            Id = internship.Id;
            Semester = internship.Semester;
            StudyYear = internship.StudyYear;
            Characteristic = internship.Characteristic;
            Mark = internship.Mark;

            Company = company;
            Intern = user;

            DiaryComments = internship.DiaryComments.Select(x => new DiaryCommentModel(x)).ToList();
        }
    }
}
