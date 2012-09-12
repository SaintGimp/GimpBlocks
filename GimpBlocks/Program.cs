using System;
using StructureMap;

namespace GimpBlocks
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            Bootstrapper.BootstrapStructureMap();

            using (var game = ObjectFactory.GetInstance<Game>())
            {
                game.Run();
            }
        }
    }
#endif
}

