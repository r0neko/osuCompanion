using System;
using System.IO;

namespace osuCompanion
{
    class Companion
    {
        static string FILE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "osuCompanion.txt");
        const string NO_MAP_SELECTED = "no map selected";

        private static MSN msn = new MSN();
        
        static private void updateFile(string d)
        {
            using (StreamWriter sw = new StreamWriter(FILE_PATH))
            {
                sw.WriteLine(string.Format("- {0} -", d));
            }
        }

        static int Main(string[] args)
        {
            Console.WriteLine("osu!companion - v1.0 by r0neko");
#if DEBUG
            Console.WriteLine("[DEBUG BUILD]");
#endif
            Console.WriteLine("https://github.com/r0neko/osuCompanion");
            Console.WriteLine();

            if (!File.Exists(FILE_PATH))
            {
                Console.WriteLine(string.Format("=> Creating '{0}'...", FILE_PATH));
                try
                {
                    File.Create(FILE_PATH);
                    updateFile("If you see this, your streaming software was configured correctly!");
                    Console.WriteLine(string.Format("Created a new file at {0}.\nPlease select '{0}' in your streaming software to show updates. In OBS, add a new Text component, then set it to 'Read from file'.", FILE_PATH));
                    Console.WriteLine("Press enter when ready.");
                    _ = Console.ReadLine();
                }
                catch (Exception)
                {
                    Console.WriteLine("[x] An error has occured while creating the file.\nPlease check that you have proper rights(are you admin?) and try again.");
                    Console.WriteLine("Press enter to exit.");
                    _ = Console.ReadLine();
                    return 1;
                }
            }
            else
            {
                Console.WriteLine(string.Format("=> File checks succeded! '{0}' exists.", FILE_PATH));
            }

            updateFile(NO_MAP_SELECTED);

            Console.WriteLine("=> Initialising MSN Handler...");
            msn.init();
            msn.MessageReceived += OnMessage;

            Console.WriteLine("=> Main updating loop starts.");

            while (true)
            {
                msn.Update();
            }
        }

        private static void OnMessage(object sender, MSN.OsuStatus status)
        {
            string template = status.status == MSN.Status.Listening ? "{0} to {1} - {2}" : "{1} - {2}";
            if (status.difficulty.Length > 0) template += "[{3}]";
            string finalString = status.status == MSN.Status.Listening ? NO_MAP_SELECTED : string.Format(template, status.status, status.title, status.artist, status.difficulty);

            Console.WriteLine("=> Status update: " + finalString);
            updateFile(finalString);
        }
    }
}
