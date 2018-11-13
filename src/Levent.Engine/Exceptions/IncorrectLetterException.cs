using System;
using System.Collections.Generic;
using System.Text;

namespace Levent.Engine
{
    public class IncorrectLetterException : Exception
    {
        public IncorrectLetterException(string message) : base(message)
        {

        }
    }
}
