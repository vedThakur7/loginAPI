using Microsoft.EntityFrameworkCore;

namespace LoginAPI.Data
{
    public class LoginAPIContext: DbContext

    {
        public LoginAPIContext(DbContextOptions<LoginAPIContext> options) : base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
    }
}
