using Microsoft.EntityFrameworkCore;

namespace ASPNETAOP_WebServer.Models
{
    public class UserLoginContext : DbContext
    {
        public UserLoginContext(DbContextOptions<UserLoginContext> options)
            : base(options)
        {
        }

        public DbSet<UserLoginItem> UserLoginItems { get; set; }
    }
}