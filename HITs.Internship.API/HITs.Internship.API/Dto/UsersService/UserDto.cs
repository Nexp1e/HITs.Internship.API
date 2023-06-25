namespace HITs.Internship.API.Dto.UsersService
{
    public class UserDto
    {
        public int Id { get; set; }
        public UserFullNameDto FullName { get; set; }
        public ICollection<string> Authorities { get; set; }
        public string? Company { get; set; }
        public string? Stream { get; set; }

        public string GetFullName() => $"{FullName.FirstName} {FullName.MiddleName} {FullName.LastName}";
    }

    public class UserFullNameDto
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
    }
}
