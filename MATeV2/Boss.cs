﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATeV2
{
    [Serializable]
    public partial class Boss: Person
    {
        public Boss(Context c,string firstname, string lastname, string mail) : base(c,firstname, lastname, mail)
        {
        }
    }
}
