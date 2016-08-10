using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    interface IEventMachine
    {
        IEvent GetEvent();
        List<IEvent> GetOutput();
    }
}
