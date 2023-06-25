using HITs.Internship.API.Data;
using HITs.Internship.API.Dto.UsersService;

namespace HITs.Internship.API.Dto.Local.Internship
{
    public class DiaryCommentModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }

        public DiaryCommentModel(Comment comment)
        {
            Id = comment.Id;
            Text = comment.Text;
            CreatedAt = comment.CreatedAt;
            AuthorName = comment.AuthorName;
        }
    }
}
