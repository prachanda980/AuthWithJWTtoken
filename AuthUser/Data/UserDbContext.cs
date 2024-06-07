using AuthUser.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthUser.Data
{
    public class UserDbContext:DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options):base(options) 
        {
            
            
        }
        public DbSet<User> Users { get; set; }
    }
}
