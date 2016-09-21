using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador
{
    class FileReader : IEventMachine<LexEvent>
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

        public LexEvent GetEvent()
        {
            int value = _file.ReadByte();
            if (value == -1)
            {
                return null;
            }
            else
            {
                var content = Convert.ToChar((byte) value);
                return new LexEvent(content);
            }
        }
    }
}
