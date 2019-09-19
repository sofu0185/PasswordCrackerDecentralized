using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master
{
    public class Passwords
    { 
        private List<UserInfo> _list;
        public string UsersAndPasswordsAsString { get; set; }
        public int UserInfoCount { get; set; }

        public Passwords()
        {
            _list = FileHandler.ReadAllPasswords();

            UsersAndPasswordsAsString = JsonConvert.SerializeObject(_list);

            Console.WriteLine("Passwords ready");
        }
    }
}
