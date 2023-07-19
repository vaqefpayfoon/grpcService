using Microsoft.EntityFrameworkCore;
using System;

namespace SeedACloud.Grpc
{

    public class CourseDbContext : DbContext
    {
        public CourseDbContext(DbContextOptions<CourseDbContext> options)
              : base(options)
        { }

        public DbSet<Course> Course { get; set; }
        public DbSet<Course.Types.Section> Section { get; set; }
        public DbSet<Rating> Rating { get; set; }
        public DbSet<Course.Types.Section.Types.Lecture> Lecture { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Course.Types.Section.Types.Lecture>().OwnsOne(l => l.Subject);
            modelBuilder.Entity<Course.Types.Section.Types.Lecture>().OwnsOne(l => l.Assignment);
        }


    }



}

