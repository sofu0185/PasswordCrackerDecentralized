using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Common;

namespace Master
{
    public class Passwords
    { 
        public List<UserInfo> PasswordList { get; set; }
        public string UsersAndPasswordsAsString { get; set; }

        public Passwords()
        {
            PasswordList = FileHandler.ReadAllPasswords();

            UsersAndPasswordsAsString = JsonConvert.SerializeObject(PasswordList);

            Console.WriteLine("\tPasswords ready");
        }

        public void SetPlainTextPassword(int id, string plainTextPassword)
        {
            PasswordList.Find(x => x.Id == id).PlainTextPassword = plainTextPassword;
        }
    }
}
