﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O campo 'Login' é obrigatório")]
        public string Login { get; set; }

        [Required(ErrorMessage = "O campo 'Senha' é obrigatório")]
        public string Senha { get; set; }
    }
}
