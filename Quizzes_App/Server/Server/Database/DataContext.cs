using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Models.QuizModel;

namespace Server.Database
{
    /// <summary>
    /// Контекст базы данных
    /// </summary>
    public class DataContext : DbContext
    {
        public DataContext()
        {

        }
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=Testdb;Integrated Security=True;MultipleActiveResultSets=True"
                ) ;

            base.OnConfiguring(optionsBuilder);
        }

        public virtual DbSet<Quiz> Quizzes { get; set; } = null!;

        public virtual DbSet<Question> Questions { get; set; } = null!;

        public virtual DbSet<Answer> Answers { get; set; } = null!;

        public virtual DbSet<User> Users { get; set; } = null!;

        public virtual DbSet<UserQuiz> UserQuizzes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserQuiz>()
           .HasKey(uq => new { uq.UserId, uq.QuizId });

            modelBuilder.Entity<Quiz>()
                .HasMany(q => q.Questions)
                .WithOne(c => c.Quiz);

            modelBuilder.Entity<Question>()
                .HasMany(q => q.Answers)
                .WithOne(c => c.Question);

            modelBuilder.Entity<Quiz>()
                .HasMany(q => q.Questions)
                .WithOne(c => c.Quiz);    
        }
    }
}
