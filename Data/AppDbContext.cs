using Microsoft.EntityFrameworkCore;
using ResetPassword.Models;
namespace ResetPassword.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<UserModel> User {  get; set; }
        public DbSet <PasswordResetToken> PasswordResetToken { get; set; }
    }
}
