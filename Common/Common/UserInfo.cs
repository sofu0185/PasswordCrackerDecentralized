using System;

namespace Common
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

        public override string ToString()
        {
            return $"{Username}: {PlainTextPassword}";
        }
    }
}
