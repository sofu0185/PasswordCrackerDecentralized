using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Master
{
    class Password
    {
        private FileStream _fs = new FileStream("passwords.txt", FileMode.Open, FileAccess.Read);

        public List<String> GetFullList()
        {
            List<String> list = new List<string>();
            StreamReader stReader = new StreamReader(_fs);
            while (!stReader.EndOfStream)
            {
                list.Add(stReader.ReadLine());
            }
            return list;
        }
    }
}
