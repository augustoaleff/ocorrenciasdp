﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.Models
{
    public class OcorrenciaViewModel
    {

        /*var relat = _db.Int_DP_Ocorrencias
               .Join(_db.Int_Dp_Usuarios, o => o.Usuario.Id, u => u.Id, (o, u) => new { o,u })
               .Select(s => new { s.o.Data,
                   s.o.Descricao, 
                   s.o.Id, 
                   s.u.Nome, 
                   s.u.Setor}).ToList();*/

        public int OcorrenciaViewModelId { get; set; }

        public DateTime Data { get; set; } //Ocorrência
         
        public string Descricao { get; set; } //Ocorrência

        public Int64 Id { get; set; } //Ocorrência

        public string Anexo { get; set; } //Ocorrência

        public string Nome { get; set; } //Setor

        public string Setor { get; set; } //Setor

        public byte Atrasado { get; set; }
        public byte Cedo { get; set; }
        public byte Advertencia { get; set; }
        public byte Acidente { get; set; }
        public byte Outro { get; set; }


    }
}
