using Microsoft.EntityFrameworkCore;
using OcorrenciasDP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Usuario> Int_Dp_Usuarios { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
