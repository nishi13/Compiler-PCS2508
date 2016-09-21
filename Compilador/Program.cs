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
            var item = lex.GetEvent();
            while (item != null && item.Token != Token.EndOfFile)
            {
                Console.WriteLine(item.Token.ToString());
                if (item.Token == Token.Id)
                {
                    Console.WriteLine(item.Content);
                }
                item = lex.GetEvent();
            }
            Console.Read();
        }
    }
}
