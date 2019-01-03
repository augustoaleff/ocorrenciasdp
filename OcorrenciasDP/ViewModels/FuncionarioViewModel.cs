using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.ViewModels
{
    public class FuncionarioViewModel
    {
        public long Id { get; set; }
        public string Nome { get; set; }
        public string Encarregado { get; set; }
        public string Loja { get; set; }
        public string Setor { get; set; }
        public byte Experiencia { get; set; }
    }
}
