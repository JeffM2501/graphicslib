﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Auth
{
    class Program
    {
        static void Main(string[] args)
        {
            new AuthServer(args[0]).Run();
        }
    }
}