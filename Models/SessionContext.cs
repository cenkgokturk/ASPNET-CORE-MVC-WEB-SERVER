using Microsoft.EntityFrameworkCore;

namespace ASPNETAOP_WebServer.Models
{
    public class SessionContext : DbContext
    {
        public SessionContext(DbContextOptions<SessionContext> options)
            : base(options)
        {
        }

        public DbSet<SessionItem> SessionItems { get; set; }
    }
}