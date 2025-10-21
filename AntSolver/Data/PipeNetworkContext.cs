
using AntSolver.Models;
using Microsoft.EntityFrameworkCore;
namespace AntSolver.Data;

public class PipeNetworkContext(DbContextOptions<PipeNetworkContext> options) : DbContext(options)
{
    public DbSet<PipeConnection> PipeConnections { get; set; }
    public DbSet<NodeRequirement> NodeRequirements { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=pipenetwork.db");
        }
    }
}
