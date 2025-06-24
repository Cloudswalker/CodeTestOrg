using LogComponent;
using LogComponent.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace LogUsers
{
    class Program
    {
        private static string? _logDirectory;
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                var logger = serviceProvider.GetRequiredService<ILog>();

                //add them all
                for (int i = 0; i < 15; i++)
                {
                    logger.Write("Number with Flush: " + i.ToString());
                }

                logger.StopWithFlush();
            }

            Console.WriteLine("Finished part 1...");
            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                ILog logger2 = serviceProvider.GetRequiredService<ILog>();

                var loggingTask = Task.Run(() =>
                {
                    for (int i = 50; i > 0; i--)
                    {
                        logger2.Write("Number with No flush: " + i.ToString());
                        Thread.Sleep(20);
                    }
                });
                //wait for a moment to ensure some logs are written
                Task.Delay(500).Wait();
                logger2.StopWithoutFlush();
                //wait for the logging task to finish
                loggingTask.Wait();
                Console.WriteLine("Finished Part 2...");
                Console.WriteLine("Finished writing logs. Press Enter to open Logs Directory.");
                Console.ReadLine();

                Process.Start("explorer.exe", _logDirectory ?? "C:\\DefaultLogDir");
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<ILogWriter>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                _logDirectory = config["Logging:Directory"] ?? "C:\\DefaultLogDir";
                return new FileLogWriter(_logDirectory);
            });
            services.AddTransient<ILog, AsyncLog>();
        }
    }
}
