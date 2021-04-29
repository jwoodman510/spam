using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace SPAM
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Consoler.WriteLines(ConsoleColor.Magenta, "Welcome to the SPAM.");

            try
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile($"appsettings.json", true, true)
                    .Build();

                var appSettings = configuration.Get<AppSettings>();

                if (!appSettings.Validate())
                {
                    Console.ReadKey();

                    return;
                }

                using var spammer = new Spammer(appSettings);

                await spammer.SpamAsync();

                Consoler.WriteLines(ConsoleColor.Green, "SPAM Complete.", "Press any key to exit.");

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Consoler.WriteError("SPAM Failed Unexpectedly.", ex.Message);
            }
        }
    }
}
