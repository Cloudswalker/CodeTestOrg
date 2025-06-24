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
            using var serviceProvider = serviceCollection.BuildServiceProvider();

            RunWithFlush(serviceProvider);
            RunWithoutFlush(serviceProvider);

            Console.WriteLine("Finished writing logs. Press Enter to open Logs Directory.");
            Console.ReadLine();
            Process.Start("explorer.exe", _logDirectory ?? "C:\\DefaultLogDir");
        }

        private static void RunWithFlush(IServiceProvider provider)
        {
            var logger = provider.GetRequiredService<ILog>();
            for (int i = 0; i < 15; i++)
            {
                logger.Write($"Number with Flush: {i}");
            }
            logger.StopWithFlush();
            Console.WriteLine("Finished part 1...");
        }

        private static void RunWithoutFlush(IServiceProvider provider)
        {
            var logger = provider.GetRequiredService<ILog>();

            var loggingTask = Task.Run(() =>
            {
                for (int i = 50; i > 0; i--)
                {
                    logger.Write($"Number with No flush: {i}");
                    Thread.Sleep(20);
                }
            });

            Task.Delay(500).Wait(); // simulate delay before stopping
            logger.StopWithoutFlush();
            loggingTask.Wait();

            Console.WriteLine("Finished part 2...");
        }


        private static void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddTransient<ILogWriter>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                _logDirectory = config["Logging:Directory"] ?? "C:\\DefaultLogDir";
                return new FileLogWriter(_logDirectory);
            });
            services.AddTransient<ILog, AsyncLog>();
        }
    }
}
