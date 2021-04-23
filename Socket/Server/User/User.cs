using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class User
    {
        public int id;
        public string username;

        public User(int _id, string _username)
        {
            id = _id;
            username = _username;

        }
    }
}
