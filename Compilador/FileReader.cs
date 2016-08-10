using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    class FileReader : IEventMachine
    {
        private FileStream _file;
        private byte[] ByteOrderMark = new byte[3] { 0xEF, 0xBB, 0xBF };
        private bool isUtf8 = true;

        public FileReader (string fileName)
        {
            _file = File.Open(fileName, FileMode.Open);
            int count = 0;
            int value;
            while (count < 3)
            {
                value = _file.ReadByte();
                if (value != ByteOrderMark[count])
                {
                    isUtf8 = false;
                    _file.Position = 0;
                    break;
                }
                count++;
            }
        }

        public IEvent GetEvent()
        {
            int value = _file.ReadByte();
            if (value == -1)
            {
                return null;
            }
            else
            {
                return new FileReaderEvent((byte) value);
            }
        }

        public List<IEvent> GetOutput()
        {
            var output = new List<IEvent>();
            FileReaderEvent ev = GetEvent() as FileReaderEvent;
            while (ev != null)
            {
                var content = Convert.ToChar(ev.Content);
                output.Add(new LexEvent(content));
                Console.Write(content);
                ev = GetEvent() as FileReaderEvent;
            }
            return output;
        }
    }
}
