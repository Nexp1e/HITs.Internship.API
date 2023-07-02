using Microsoft.EntityFrameworkCore;

namespace HITs.Internship.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Internship> Internships { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<DiaryTemplate> DiaryTemplates { get; set; }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
