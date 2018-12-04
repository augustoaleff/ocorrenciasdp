using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.Models
{
    public class ValidacaoSenha
    {
        public long Id { get; set; }
        public DateTime Data { get; set; }
        public DateTime DataExpiracao { get; set; }
        public byte Utilizado { get; set; }
        public int Usuario { get; set; }

    }
}
