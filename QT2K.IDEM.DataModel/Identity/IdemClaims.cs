using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT2K.IDEM.DataModel.Identity
{
    public class IdemClaims
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Roles { get; set; }
        public string Cognome { get; set; }
        public string Nome { get; set; }
    }
}
