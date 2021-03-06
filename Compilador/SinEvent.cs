using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    class SinEvent : IEvent
    {
        public SinEvent(Token token, object content = null)
        {
            Token = token;
            Content = content;
        }
        public Token Token { get; }
        public object Content { get; }

        public override string ToString()
        {
            if (Content != null)
            {
                return "Token:" + Token.ToString() + " Value:" + Content.ToString();
            }
            else
            {
                return "Token:" + Token.ToString();
            }
        }
    }
}
