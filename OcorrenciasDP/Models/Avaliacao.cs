using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.Models
{
    public class Avaliacao
    {
        public long Id { get; set; }
        public Funcionario Funcionario { get; set; }
        public DateTime DataAvaliacao { get; set; }
        public Usuario Encarregado { get; set; }
        public int Nota { get; set; }
        public string Comentario { get; set; }

    }
}
