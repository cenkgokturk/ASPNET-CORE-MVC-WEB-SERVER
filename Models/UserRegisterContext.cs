using Microsoft.EntityFrameworkCore;

namespace ASPNETAOP_WebServer.Models
{
    public class UserRegisterContext : DbContext
    {
        public UserRegisterContext(DbContextOptions<UserRegisterContext> options)
            : base(options)
        {
        }

        public DbSet<UserRegisterItem> UserRegisterItems { get; set; }
    }
}