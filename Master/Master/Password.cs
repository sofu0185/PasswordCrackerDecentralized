using System;
using System.Collections.Generic;
using System.IO;

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

        public string GetPass()
        {
            List<String> list = GetFullList();
            return list[_currentPassIndex];
        }

        public void NextPass()
        {
            List<String> list = GetFullList();

            if (_currentPassIndex  == list.Count-1)
            {
                _currentPassIndex = 0;
            }
            else
            {
                _currentPassIndex++;
            }
        }
    }
}
