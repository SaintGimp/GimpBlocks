﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public interface IListener<in T>
    {
        void Handle(T message);
    }
}
