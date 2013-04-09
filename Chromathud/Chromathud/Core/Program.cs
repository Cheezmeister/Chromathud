using System;

namespace ChromathudWin
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {           
            using (ChromathudGame game = new ChromathudGame())
            {
                game.Run();
            }
        }

    }
}

