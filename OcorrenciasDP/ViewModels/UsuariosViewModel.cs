using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.ViewModels
{
    public class UsuariosViewModel
    {
        public int Id { get; set; }

        public string Login { get; set; }

        public string Nome { get; set; }

        public string Setor { get; set; }

        public string Perfil { get; set; }

        public byte Ativo { get; set; }

        public DateTime UltimoAcesso { get; set; }
       
    }
}
