using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class MigrationStep
    {
        public string NombrePaso { get; set; }
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
    }
}
