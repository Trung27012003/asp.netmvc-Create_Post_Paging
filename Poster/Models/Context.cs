using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Poster.Models
{
    public class Context:DbContext
    {
        public Context()
        {

        }
        public Context(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Posts> Posts { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=TRUNG2701\SQLEXPRESS;Initial Catalog=Demo_Posts;Integrated Security=True;Connection Timeout=36000");// \SQLEXPRESS
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
