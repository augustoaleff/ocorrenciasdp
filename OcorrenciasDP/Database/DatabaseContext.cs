﻿using Microsoft.EntityFrameworkCore;
using OcorrenciasDP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Usuario> Int_DP_Usuarios { get; set; }
        public DbSet<Ocorrencia> Int_DP_Ocorrencias { get; set; }
        public DbSet<Setor> Int_DP_Setores { get; set; }
        public DbSet<Mensagem> Int_DP_Mensagens { get; set; }
        public DbSet<LogTipos> Int_DP_Logs_Tipos { get; set; }
        public DbSet<Log> Int_DP_Logs { get; set; }
        public DbSet<Feriado> Int_DP_Feriados { get; set; }
        public DbSet<Funcionario> Int_DP_Funcionarios { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
