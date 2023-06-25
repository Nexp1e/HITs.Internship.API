namespace HITs.Internship.API.Data
{
    public class Comment
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Text { get; set; }

        public string AuthorName { get; set; }
        public int AuthorId { get; set; }
        public int InternshipId { get; set; }
        public Internship Internship { get; set; }
    }
}
