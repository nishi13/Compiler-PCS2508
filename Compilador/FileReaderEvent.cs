using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    class FileReaderEvent : IEvent
    {
        public FileReaderEvent (byte content)
        {
            Content = content;
        }
        public byte Content { get; }
    }
}
