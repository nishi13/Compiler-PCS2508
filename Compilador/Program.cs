using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileReader = new FileReader("program-test.txt");
            var lex = new Lex(fileReader);
            var sin = new Sin(lex);
            Console.WriteLine(sin.GetEvent());
            Console.Read();
        }
    }
}
