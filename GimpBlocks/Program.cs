using System;

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

            using (Game game = new Game())
            {
                game.Run();
            }
        }
    }
#endif
}

