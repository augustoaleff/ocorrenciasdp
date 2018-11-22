using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.Models
{
    public class Ocorrencia
    {
        public long Id { get; set; }


        [Required(ErrorMessage ="Esse Campo é Obrigatório")]
        public DateTime Data { get; set; }

        public DateTime DataEnvio { get; set; }
       
        [MaxLength(3000, ErrorMessage ="Este campo deve conter no máximo 3000 caracteres")]
        public string Descricao { get; set; }

        public string Anexo { get; set; }

        public Usuario Usuario { get; set; }


        public byte Atrasado { get; set; }
        public byte Cedo { get; set; }
        public byte Advertencia { get; set; }
        public byte Acidente { get; set; }
        public byte Outro { get; set; }

        

        public Ocorrencia()
        {
            this.Atrasado = 2;
            this.Cedo = 2;
            this.Advertencia = 2;
            this.Acidente = 2;
            this.Outro = 2;
        }

    }
}
