using System;
using System.Collections.Generic;
using System.Text;

namespace Slave
{
    public class UserInfo
    {
        public string Username { get; set; }
        public string HashedPassword { get; set; }
        public string PlainTextPassword { get; set; }

        public UserInfo() { }

        public UserInfo(string username, string hashedpass, string plainTextPass = null)
        {
            Username = username;
            HashedPassword = hashedpass;
        }
    }
}
