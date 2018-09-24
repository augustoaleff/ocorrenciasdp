using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.ViewModels
{
    public class FiltrarPesquisaRelatViewModel
    {
        public DateTime? DataInicio { get; set; }

        public DateTime? DataFim { get; set; }

        public string Setor { get; set; }

    }
}
