using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.Models
{
    public class Mensagem
    {
        public long Id { get; set; }

        [MaxLength(50, ErrorMessage ="O Tamanho permitido do título é de 50 Caracteres")]
        public string Titulo { get; set; }
        public string Conteudo { get; set; }
        public DateTime Data { get; set; }
        public Usuario Remetente { get; set; }
    }
}
