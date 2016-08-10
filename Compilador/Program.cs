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
            var output = fileReader.GetOutput();
            Console.Read();
        }
    }
}
