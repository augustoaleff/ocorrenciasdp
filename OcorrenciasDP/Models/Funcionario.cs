using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.Models
{
    public class Funcionario
    {
        [Key]
        public long Id { get; set; }

        [MaxLength(50, ErrorMessage = "O nome deve possuir no máximo 50 caracteres")]
        public string Nome { get; set; }
        public Usuario Encarregado { get; set; }
        public Loja Loja { get; set; }
        public Setor Setor { get; set; }
        public DateTime DataCadastro { get; set; }
        public Usuario CadastradoPor { get; set; } //Qual o usuário que cadastrou o funcionário.
        public byte Ativo { get; set; }
        

        //Experiência
        public byte Experiencia { get; set; }
        public DateTime Exp_DataInicio { get; set; }
        public DateTime Exp_DataFim { get; set; }

        //Férias
        public DateTime Ferias_DataLimite { get; set; }
        public DateTime Ferias_DataInicio { get; set; }
        public int Ferias_Periodo { get; set; }
    }
}
