using System.ComponentModel.DataAnnotations;

namespace HITs.Internship.API.Dto.Local.Internship
{
    public class PostDiaryCommentModel
    {
        [Required]
        public string Text { get; set; }
    }
}
