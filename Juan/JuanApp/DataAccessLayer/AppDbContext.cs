using JuanApp.Models;
using Microsoft.EntityFrameworkCore;

namespace JuanApp.DataAccessLayer;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Setting> Settings { get; set; }
}