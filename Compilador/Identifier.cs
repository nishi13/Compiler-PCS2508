using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    class Identifier
    {
        string Name { get; set; }
        Token Type { get; set; }
        object Value { get; set; }

        public Identifier(string name)
        {
            Name = name;
        }
    }
}
