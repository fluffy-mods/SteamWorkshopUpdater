using System;
using System.Threading;
using Steamworks;
using SteamWorkshopUploader;

namespace SteamWorkshopUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
                Console.WriteLine(i + ": " + args[i]);

            try
            {
                var mod = new Mod( args[0] );
                Uploader.Init();
                Console.WriteLine( mod.ToString() );
                if ( Uploader.Upload( mod ) )
                {
                    Console.WriteLine("Upload done");
                }
            }
            catch ( Exception e )
            {
                Console.WriteLine( e.Message );
            }
            finally
            {
                Uploader.Shutdown();
            }

            Console.WriteLine( "Done. Press any key to exit." );
            Console.ReadKey();
        }
    }
}
