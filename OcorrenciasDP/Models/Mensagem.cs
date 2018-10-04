using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.Models
{
    public class Mensagem
    {
        public long Id { get; set; }
        public string Conteudo { get; set; }
        public DateTime Data { get; set; }
        public Usuario Remetente { get; set; }
    }
}
