namespace HITs.Internship.API.Dto.UsersService
{
    public class CompanyDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OfficialName { get; set; }
        public string? Contacts { get; set; }
        public UserDto Supervisor { get; set; }
        public UserDto Representative { get; set; }
    }
}
