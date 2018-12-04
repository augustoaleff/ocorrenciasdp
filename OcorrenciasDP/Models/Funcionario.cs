using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.Models
{
    public class Funcionario
    {
        public long Id { get; set; }
        public string Nome { get; set; }
        public Usuario Encarregado { get; set; }
        public Setor Setor { get; set; }
        public DateTime DataCadastro { get; set; }
        public Usuario CadastradoPor { get; set; } //Qual o usuário que cadastrou o funcionário.
        public byte Ativo { get; set; }
        public int Loja { get; set; }

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
