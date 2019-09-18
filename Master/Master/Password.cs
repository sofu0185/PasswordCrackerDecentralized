using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Master
{
    class Password
    { 
        private int _currentPassIndex;
        private List<String> _list;

        public List<String> GetFullList()
        {
            using (FileStream _fs = new FileStream("passwords.txt", FileMode.Open, FileAccess.Read))
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

        public string GetNextPass()
        {
            List<String> list = GetFullList();
            String pass = "";

            if (list.Count > _currentPassIndex)
            {
                pass = list[_currentPassIndex];
                _currentPassIndex++;
                return pass;
            }
            else
            {
                return "No more passwords";
            }

        }
    }
}
