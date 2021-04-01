using Microsoft.EntityFrameworkCore;
using ASPNETAOP_WebServer.Models;

namespace ASPNETAOP_WebServer.Models
{
    public class UserRegisterContext : DbContext
    {
        public UserRegisterContext(DbContextOptions<UserRegisterContext> options)
            : base(options)
        {
        }

        public DbSet<UserRegisterItem> UserRegisterItems { get; set; }

        public DbSet<ASPNETAOP_WebServer.Models.UserInfoItem> UserInfoItem { get; set; }
    }
}