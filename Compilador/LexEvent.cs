using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    class LexEvent : IEvent
    {
        public LexEvent(char content)
        {
            Content = content;
        }
        public char Content { get; }
    }
}
