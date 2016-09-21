using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    class Sin : IEventMachine<bool>
    {
        private IEventMachine<SinEvent> _provider;
        private List<SinEvent> _savedEvents;
        private bool _sintaxError = false;

        public Sin(IEventMachine<SinEvent> provider)
        {
            _provider = provider;
            _savedEvents = new List<SinEvent>();
        }

        public bool GetEvent()
        {
            return Program();
        }

        private SinEvent NextEvent()
        {
            if (_savedEvents.Count > 0)
            {
                var ret = _savedEvents.First();
                _savedEvents.RemoveAt(0);
                return ret;
            }
            else
            {
                return _provider.GetEvent();
            }
        }

        private bool Program()
        {
            if (!DefPart() || _sintaxError) return false;
            var content = NextEvent();
            if (content.Token == Token.EndOfFile)
            {
                return true;
            }
            else {
                error("Unexpected " + content.ToString());
                return false;
            }
        }

        private bool DefPart()
        {
            while (VarDef() || FuncDef()) {}
            return true;
        }

        private bool VarDef()
        {
            var type = NextEvent();
            if (type.Token != Token.IntDeclaration &&
                type.Token != Token.BooleanDeclaration &&
                type.Token != Token.FloatDeclaration)
            {
                _savedEvents.Add(type);
                return false;
            }
            var next = NextEvent();
            if (next.Token == Token.OpenBracket)
            {
                var n = Number();
                if (!n.HasValue)
                {
                    error("Expected Number but got " + NextEvent().Token);
                    return false;
                }
            }
            return true;
        }
        private int? Number()
        {
            return null;
        }

        private void error(string msg)
        {
            _sintaxError = true;
            Console.WriteLine("Sintax Error: " + msg);
        }
    }
}
