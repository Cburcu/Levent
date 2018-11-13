using System;

namespace Levent.Engine
{
    public class User
    {
        public string Username { get; set; }
        public char[,] Grid { get; set; }
        public User(string user1Name, int x, int y)
        {
            this.Username = user1Name;
            this.Grid = new char[x, y];
        }
    }
}
