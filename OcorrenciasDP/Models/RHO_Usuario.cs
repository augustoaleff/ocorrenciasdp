using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.Models
{
    public class RHO_Usuario
    {

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O Campo 'Login' é Obrigatório")]
        [MaxLength(50, ErrorMessage = "O login deve conter menos que 50 caracteres!")]
        public string CPF { get; set; }

        [Required(ErrorMessage = "O Campo 'Senha' é Obrigatório")]
        [MinLength(8, ErrorMessage = "O senha deve ser maior que 8 caracteres!")]
        [MaxLength(50, ErrorMessage = "O login deve conter menos que 50 caracteres!")]
        public string Senha { get; set; }

        [Required(ErrorMessage = "O Campo 'Nome' é Obrigatório")]
        [MaxLength(50, ErrorMessage = "O Campo 'Nome' deve ter menos que 50 caracteres")]
        public string Nome { get; set; }

        [Phone(ErrorMessage = "Insira um numero de telefone válido")]
        public string Telefone { get; set; }

        [Phone(ErrorMessage = "Insira um numero de telefone válido")]
        public string Celular { get; set; }

        [EmailAddress(ErrorMessage = "Insira um email válido!")]
        public string Email { get; set; }

        public byte Nivel { get; set; }
        //Nivel 1: Administrador;
        //Nível 2: Usuário;
        //Nivel 3: Suporte (TI);

        public DateTime DataCadastro { get; set; }

        public DateTime UltimoAcesso { get; set; }

        public Loja Loja { get; set; }

        public Setor Setor { get; set; }

        public byte Ativo { get; set; }
        // 0  -> Não
        // 1  -> Sim

        public int CodigoAtivacao { get; set; }
        //Código Gerado

        public byte Cadastrado { get; set; }
        // 0  -> Não
        // 1  -> Sim
    }
}
