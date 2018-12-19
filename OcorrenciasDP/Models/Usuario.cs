using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O campo 'Login' é obrigatório")]
        [MaxLength(50, ErrorMessage ="O login deve conter no máximo 50 caracteres")]
        public string Login { get; set; }

        [Required(ErrorMessage = "O campo 'Senha' é obrigatório")]
        [MaxLength(50, ErrorMessage = "A senha deve conter no máximo 50 caracteres")]
        public string Senha { get; set; }

        public byte Ativo { get; set; }

        [MaxLength(50, ErrorMessage = "O nome deve possuir no máximo 50 caracteres")]
        public string Nome { get; set; }
        public string Perfil { get; set; }
        public Setor Setor { get; set; }
        public DateTime UltimoLogin { get; set; } // o Último login Efetuado
        public DateTime DataCadastro { get; set; }
        public Loja Loja { get; set; }
       
        public string Email { get; set; }

    }
}
