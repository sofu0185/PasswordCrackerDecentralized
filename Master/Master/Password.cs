using System;
using System.Collections.Generic;
using System.IO;

namespace Master
{
    class Password
    { 
        private int _currentPassIndex;
        private List<String> _list;

        public Password()
        {
         GetFullList();   
        }

        public void GetFullList()
        {
            using (FileStream _fs = new FileStream("../../../../../passwords.txt", FileMode.Open, FileAccess.Read))
            {
                _list = new List<string>();
                StreamReader stReader = new StreamReader(_fs);
                while (!stReader.EndOfStream)
                {
                    _list.Add(stReader.ReadLine());
                }
            }
        }

        public string GetPass()
        {
            return _list[_currentPassIndex].Split(":")[1];
        }

        public string GetName()
        {
            return _list[_currentPassIndex].Split(":")[0];
        }

        public bool NextPass()
        {
            _currentPassIndex++;
            if (_currentPassIndex == _list.Count)
            {
                return true;
            }

            return false;
        }
    }
}
