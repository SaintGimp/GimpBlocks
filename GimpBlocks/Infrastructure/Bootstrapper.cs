﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StructureMap;

namespace GimpBlocks
{
    public static class Bootstrapper
    {
        public static void BootstrapStructureMap()
        {
            ObjectFactory.Initialize(x => x.AddRegistry<GameRegistry>());
        }
    }
}
