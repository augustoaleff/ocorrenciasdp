using System;
using System.Collections.Generic;
using System.Linq;
using OcorrenciasDP.Models;
using System.Threading.Tasks;

namespace OcorrenciasDP.ViewModels
{
    public class DetalhesFuncViewModel
    {
        public Funcionario Funcionario { get; set; }
        public List<Avaliacao> Avaliacoes { get; set; }
        public Usuario Usuario { get; set; }
        public DateTime Data { get; set; }

    }
}
