using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.Models
{
    public class Ocorrencia
    {
        public Int64 Id { get; set; }

        //public int? Id_usuario { get; set; }

        [Required(ErrorMessage ="Esse Campo é Obrigatório")]
        public DateTime Data { get; set; }

        [Required(ErrorMessage ="Este campo é Obrigatório")]
        [MaxLength(3000, ErrorMessage ="Este campo deve conter no máximo 3000 caracteres")]
        public string Descricao { get; set; }

        public string Anexo { get; set; }

        public Usuario Usuario { get; set; }
    }
}
