using System;
using System.Collections.Generic;

namespace Master
{
    public class Passwords
    { 
        private int _currentPassIndex;
        private List<UserInfo> _list;
        public string UsersAndPasswordsAsString { get; set; }
        public int UserInfoCount { get; set; }

        public Passwords()
        {
            _list = FileHandler.ReadAllPasswords();

            UsersAndPasswordsAsString = "";
            foreach(UserInfo ui in _list)
            {
                UsersAndPasswordsAsString += $"{ui.Username}:{ui.HashedPassword},";
            }
            UsersAndPasswordsAsString = UsersAndPasswordsAsString.TrimEnd(',');
        }
    }
}
