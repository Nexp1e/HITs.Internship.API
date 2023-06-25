using HITs.Internship.API.Dto.UsersService;

namespace HITs.Internship.API.Dto.Local.Internship
{
    public class InternshipPreview
    {
        public int Id { get; set; }
        public int Semester { get; set; }
        public int StudyYear { get; set; }
        public double? Mark { get; set; }
        public string? Characteristic { get; set; }
        public UserDto Intern { get; set; }

        public InternshipPreview() { }

        public InternshipPreview(Data.Internship internship, UserDto user)
        {
            Id = internship.Id;
            Semester = internship.Semester;
            StudyYear = internship.StudyYear;
            Characteristic = internship.Characteristic;
            Mark = internship.Mark;

            Intern = user;
        }
    }
}
