using System;
using System.Collections.Generic;
using System.Text;

namespace Levent.Engine
{
    public class UserResult
    {
        public User User { get; set; }
        public List<string> MeaningfulWords { get; set; }
        public int Score { get; set; }
    }
}
