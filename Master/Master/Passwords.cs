using System;
using System.Collections.Generic;

namespace Master
{
    public class Passwords
    { 
        private int _currentPassIndex;
        private List<UserInfo> _list;

        public Passwords()
        {
            _list = FileHandler.ReadAllPasswords();   
        }

        public string GetPass()
        {
            return _list[_currentPassIndex].HashedPassword;
        }

        public string GetName()
        {
            return _list[_currentPassIndex].Username;
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
