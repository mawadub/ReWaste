using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RE_Waste.Models;

namespace RE_Waste.Data
{
    // This class represents the application's database context, which is responsible for managing the connection to the database and providing access to the application's data models
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // DbSet properties represent the tables in the database. Each DbSet corresponds to a model class
        public DbSet<Question> Questions => Set<Question>();
        public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
        public DbSet<DropOffPoint> DropOffPoints => Set<DropOffPoint>();
        public DbSet<RecommendationRule> RecommendationRules => Set<RecommendationRule>();
        public DbSet<SavedResponse> SavedResponses => Set<SavedResponse>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
