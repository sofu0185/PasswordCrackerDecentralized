using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Common;

namespace Master
{
    public static class FileHandler
    {
        public static Span<string> ReadAllWordsInDictionarySpan()
        {
            return File.ReadAllLines(Constants.DICTIONARY_PATH);
        }

        public static List<UserInfo> ReadAllPasswords()
        {
            List<UserInfo> result = new List<UserInfo>();

            string[] passlist = File.ReadAllLines(Constants.PASSWORD_PATH);
            foreach(string s in passlist)
            {
                string[] passAndName = s.Split(':');
                result.Add(new UserInfo(passAndName[0], passAndName[1]));
            }

            return result;
        }
    }
}
