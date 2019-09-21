using System;

namespace Common
{
    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string HashedPassword { get; set; }
        public string PlainTextPassword { get; set; }

        public UserInfo() { }
        public UserInfo(string username, string hashedpass)
        {
            Username = username;
            HashedPassword = hashedpass;
        }
        public UserInfo(int id, string username, string hashedpass)
            :this(username, hashedpass)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"{Username}: {PlainTextPassword}";
        }
    }
}
