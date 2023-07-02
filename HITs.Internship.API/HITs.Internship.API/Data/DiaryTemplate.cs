using System.ComponentModel.DataAnnotations;

namespace HITs.Internship.API.Data
{
    public class DiaryTemplate
    {
        [Key]
        public int Semester { get; set; }
        public string Order { get; set; }
        public byte[] File { get; set; }
    }
}
