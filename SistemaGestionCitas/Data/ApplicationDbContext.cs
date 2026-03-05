using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaGestionCitas.Models;

namespace SistemaGestionCitas.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Cliente> Clientes { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
