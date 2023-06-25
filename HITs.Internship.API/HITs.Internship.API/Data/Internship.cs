using System.ComponentModel.DataAnnotations.Schema;

namespace HITs.Internship.API.Data
{
    public class Internship
    {
        public int Id { get; set; }
        public int Semester { get; set; }
        public int StudyYear { get; set; }
        public double? Mark { get; set; }
        public string? Characteristic { get; set; }
        public byte[]? Diary { get; set; }

        public ICollection<Comment> DiaryComments { get; set; }
        public int CompanyId { get; set; }
        public int InternId { get; set; }

        
        public static int GetCurrentStudyYear()
        {
            var curDate = DateTime.UtcNow;
            if (curDate.Month >= 9)
            {
                return curDate.Year;
            }

            return curDate.Year - 1;
        }
    }
}
