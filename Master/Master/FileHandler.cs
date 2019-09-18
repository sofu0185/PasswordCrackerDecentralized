using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Master
{
    public static class FileHandler
    {
        private const string _dictionaryPath = "Text files/webster-dictionary.txt";
        private const string _passwordPath = "Text files/passwords.txt";

        public static List<string> ReadAllWordsInDictionary()
        {
            return File.ReadAllLines(_dictionaryPath).ToList();
        }

        public static List<UserInfo> ReadAllPasswords()
        {
            List<UserInfo> result = new List<UserInfo>();

            string[] passlist = File.ReadAllLines(_passwordPath);
            foreach(string s in passlist)
            {
                string[] passAndName = s.Split(':');
                result.Add(new UserInfo(passAndName[0], passAndName[1]));
            }

            return result;
        }
    }
}
